// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;

    public interface IGoogleMusicSynchronizationService
    {
        Task<UpdateStatus> Update(IProgress<double> progress = null);
    }

    public class UpdateStatus
    {
        public bool Updated { get; private set; }
        public bool IsBreakingChange { get; private set; }

        public void SetUpdated()
        {
            this.Updated = true;
        }

        public void SetBreakingChange()
        {
            this.Updated = true;
            this.IsBreakingChange = true;
        }
    }

    public class GoogleMusicSynchronizationService : RepositoryBase, IGoogleMusicSynchronizationService
    {
        private readonly ILogger logger;
        private readonly ISettingsService settingsService;
        private readonly IPlaylistsWebService playlistsWebService;
        private readonly ISongsWebService songsWebService;
        private readonly IRadioWebService radioWebService;
        private readonly IUserPlaylistsRepository userPlaylistsRepository;
        private readonly ISongsRepository songsRepository;
        private readonly IRadioStationsRepository radioStationsRepository;
        private readonly IConfigWebService configWebService;

        public GoogleMusicSynchronizationService(
            ILogManager logManager,
            ISettingsService settingsService,
            IPlaylistsWebService playlistsWebService,
            ISongsWebService songsWebService,
            IRadioWebService radioWebService,
            IUserPlaylistsRepository userPlaylistsRepository,
            ISongsRepository songsRepository,
            IRadioStationsRepository radioStationsRepository,
            IConfigWebService configWebService)
        {
            this.logger = logManager.CreateLogger("GoogleMusicSynchronizationService");
            this.settingsService = settingsService;
            this.playlistsWebService = playlistsWebService;
            this.songsWebService = songsWebService;
            this.radioWebService = radioWebService;
            this.userPlaylistsRepository = userPlaylistsRepository;
            this.songsRepository = songsRepository;
            this.radioStationsRepository = radioStationsRepository;
            this.configWebService = configWebService;
        }

        public async Task<UpdateStatus> Update(IProgress<double> progress = null)
        {
            UpdateStatus updateStatus = new UpdateStatus();

            DateTime? libraryFreshnessDate = this.settingsService.GetLibraryFreshnessDate();
            DateTime currentTime = DateTime.UtcNow;

            IProgress<int> subProgress = null;

            // Get status if we need to report progress

            if (!libraryFreshnessDate.HasValue && progress != null)
            {
                var status = await this.songsWebService.GetStatusAsync();
                subProgress = new Progress<int>(async songsCount => await progress.SafeReportAsync((((double)songsCount / status.AvailableTracks) * 0.75d) + 0.05d));
            }

            await progress.SafeReportAsync(0.03d);

            var allAccess = await this.configWebService.IsAllAccessAvailableAsync();
            if (this.settingsService.GetIsAllAccessAvailable() != allAccess)
            {
                updateStatus.SetBreakingChange();
                this.settingsService.SetIsAllAccessAvailable(allAccess);
            }

            await progress.SafeReportAsync(0.05d);

            // If this is not an initial load - let's send statistics first
            if (libraryFreshnessDate.HasValue)
            {
                var songsForStat = await this.songsRepository.GetSongsForStatUpdateAsync();

                if (songsForStat.Count > 0)
                {
                    var result = await this.songsWebService.SendStatsAsync(songsForStat);
                    foreach (var response in result.Responses)
                    {
                        if (string.Equals(response.ResponseCode, "OK", StringComparison.OrdinalIgnoreCase))
                        {
                            var song =
                                songsForStat.FirstOrDefault(
                                    x => string.Equals(x.SongId, response.Id, StringComparison.OrdinalIgnoreCase));

                            await this.songsRepository.ResetStatsAsync(song);
                        }
                    }
                }
            }

            await this.songsWebService.GetAllAsync(
                libraryFreshnessDate,
                subProgress,
                async (gSongs) =>
                {
                    IList<Song> toBeDeleted = new List<Song>();
                    IList<Song> toBeUpdated = new List<Song>();
                    IList<Song> toBeInserted = new List<Song>();

                    foreach (var googleMusicSong in gSongs)
                    {
                        if (googleMusicSong.Deleted)
                        {
                            toBeDeleted.Add(googleMusicSong.ToSong());
                            
                        }
                        else
                        {
                            Song song = null;
                            if (libraryFreshnessDate.HasValue)
                            {
                                song = await this.songsRepository.FindSongAsync(googleMusicSong.Id);
                            }

                            if (song != null)
                            {
                                GoogleMusicSongEx.Mapper(googleMusicSong, song);
                                toBeUpdated.Add(song);

                                if (!GoogleMusicSongEx.IsVisualMatch(googleMusicSong, song))
                                {
                                    updateStatus.SetBreakingChange();
                                }
                                else
                                {
                                    updateStatus.SetUpdated();
                                }
                            }
                            else
                            {
                                toBeInserted.Add(googleMusicSong.ToSong());
                            }
                        }
                    }

                    if (toBeDeleted.Count > 0)
                    {
                        if (await this.songsRepository.DeleteAsync(toBeDeleted) > 0)
                        {
                            updateStatus.SetBreakingChange();
                        }
                    }

                    if (toBeInserted.Count > 0)
                    {
                        if (await this.songsRepository.InsertAsync(toBeInserted) > 0)
                        {
                            updateStatus.SetBreakingChange();
                        }
                    }

                    if (toBeUpdated.Count > 0)
                    {
                        await this.songsRepository.UpdateAsync(toBeUpdated);
                    }
                });

            await progress.SafeReportAsync(0.8d);

            this.logger.Debug("LoadPlaylistsAsync: loading playlists.");
            await this.playlistsWebService.GetAllAsync(libraryFreshnessDate, chunkHandler: async (chunk) =>
            {
                IList<UserPlaylist> toBeDeleted = new List<UserPlaylist>();
                IList<UserPlaylist> toBeUpdated = new List<UserPlaylist>();
                IList<UserPlaylist> toBeInserted = new List<UserPlaylist>();

                foreach (var googleMusicPlaylist in chunk.Where(x => string.Equals(x.Type, "USER_GENERATED", StringComparison.OrdinalIgnoreCase)))
                {
                    if (googleMusicPlaylist.Deleted)
                    {
                        toBeDeleted.Add(googleMusicPlaylist.ToUserPlaylist());
                    }
                    else
                    {
                        UserPlaylist currentPlaylist = null;
                        if (libraryFreshnessDate.HasValue)
                        {
                            currentPlaylist = await this.userPlaylistsRepository.GetAsync(googleMusicPlaylist.Id);
                        }

                        if (currentPlaylist != null)
                        {
                            GoogleMusicPlaylistEx.Mapper(googleMusicPlaylist, currentPlaylist);
                            toBeUpdated.Add(currentPlaylist);

                        }
                        else
                        {
                            toBeInserted.Add(googleMusicPlaylist.ToUserPlaylist());
                        }
                    }
                }

                if (toBeDeleted.Count > 0)
                {
                    if (await this.userPlaylistsRepository.DeleteAsync(toBeDeleted) > 0)
                    {
                        updateStatus.SetBreakingChange();
                    }
                }

                if (toBeInserted.Count > 0)
                {
                    if (await this.userPlaylistsRepository.InsertAsync(toBeInserted) > 0)
                    {
                        updateStatus.SetBreakingChange();
                    }
                }

                if (toBeUpdated.Count > 0)
                {
                    await this.userPlaylistsRepository.UpdateAsync(toBeUpdated);
                }
            });

            await progress.SafeReportAsync(0.85d);

            this.logger.Debug("LoadPlaylistsAsync: loading playlist entries.");
            await this.playlistsWebService.GetAllPlaylistEntries(libraryFreshnessDate, chunkHandler: async (chunk) =>
            {
                IList<UserPlaylistEntry> toBeDeleted = new List<UserPlaylistEntry>();
                IList<UserPlaylistEntry> toBeUpdated = new List<UserPlaylistEntry>();
                IList<UserPlaylistEntry> toBeInserted = new List<UserPlaylistEntry>();

                IDictionary<string, Song> songsToInsert = new Dictionary<string, Song>();
                IDictionary<string, Song> songsToUpdate = new Dictionary<string, Song>();

                foreach (var entry in chunk)
                {
                    if (entry.Deleted)
                    {
                        toBeDeleted.Add(entry.ToUserPlaylistEntry());
                    }
                    else
                    {
                        UserPlaylistEntry currentEntry = null;
                        if (libraryFreshnessDate.HasValue)
                        {
                            currentEntry = await this.userPlaylistsRepository.GetEntryAsync(entry.Id);
                        }

                        if (currentEntry != null)
                        {
                            GoogleMusicPlaylistEntryEx.Mapper(entry, currentEntry);
                            toBeUpdated.Add(currentEntry);
                        }
                        else
                        {
                            toBeInserted.Add(entry.ToUserPlaylistEntry());
                        }

                        if (entry.Track != null)
                        {
                            Song currentSong = entry.Track.ToSong();
                            if (libraryFreshnessDate.HasValue)
                            {
                                currentSong = await this.songsRepository.FindSongAsync(currentSong.SongId);
                            }
                            
                            if (currentSong != null)
                            {
                                if (!songsToUpdate.ContainsKey(currentSong.SongId))
                                {
                                    GoogleMusicSongEx.Mapper(entry.Track, currentSong);
                                    songsToUpdate.Add(currentSong.SongId, currentSong);
                                }
                            }
                            else
                            {
                                currentSong = entry.Track.ToSong();
                                if (!songsToInsert.ContainsKey(currentSong.SongId))
                                {
                                    currentSong.IsLibrary = false;
                                    songsToInsert.Add(currentSong.SongId, currentSong);
                                }
                            }
                        }
                    }
                }

                if (songsToInsert.Count > 0)
                {
                    await this.songsRepository.InsertAsync(songsToInsert.Values);
                }

                if (songsToUpdate.Count > 0)
                {
                    await this.songsRepository.UpdateAsync(songsToUpdate.Values);
                }

                if (toBeDeleted.Count > 0)
                {
                    if (await this.userPlaylistsRepository.DeleteEntriesAsync(toBeDeleted) > 0)
                    {
                        updateStatus.SetBreakingChange();
                    }
                }

                if (toBeInserted.Count > 0)
                {
                    if (await this.userPlaylistsRepository.InsertEntriesAsync(toBeInserted) > 0)
                    {
                        updateStatus.SetBreakingChange();
                    }
                }

                if (toBeUpdated.Count > 0)
                {
                    await this.userPlaylistsRepository.UpdateEntriesAsync(toBeUpdated);
                }
            });

            await progress.SafeReportAsync(0.95d);
            
            await this.radioWebService.GetAllAsync(
                libraryFreshnessDate,
                null,
                async (gRadios) =>
                {
                    IList<Radio> toBeDeleted = new List<Radio>();
                    IList<Radio> toBeUpdated = new List<Radio>();
                    IList<Radio> toBeInserted = new List<Radio>();

                    foreach (var radio in gRadios)
                    {
                        if (radio.Deleted)
                        {
                            toBeDeleted.Add(radio.ToRadio());
                        }
                        else
                        {
                            Radio storedRadio = null;
                            if (libraryFreshnessDate.HasValue)
                            {
                                storedRadio = await this.radioStationsRepository.GetAsync(radio.Id);
                            }

                            if (storedRadio != null)
                            {
                                GoogleMusicRadioEx.Mapper(radio, storedRadio);
                                toBeUpdated.Add(storedRadio);

                            }
                            else
                            {
                                toBeInserted.Add(radio.ToRadio());
                            }
                        }
                    }

                    if (toBeDeleted.Count > 0)
                    {
                        if (await this.radioStationsRepository.DeleteAsync(toBeDeleted) > 0)
                        {
                            updateStatus.SetBreakingChange();
                        }
                    }

                    if (toBeInserted.Count > 0)
                    {
                        if (await this.radioStationsRepository.InsertAsync(toBeInserted) > 0)
                        {
                            updateStatus.SetBreakingChange();
                        }
                    }

                    if (toBeUpdated.Count > 0)
                    {
                        await this.radioStationsRepository.UpdateAsync(toBeUpdated);
                    }
                });
            

            await progress.SafeReportAsync(1d);

            this.settingsService.SetLibraryFreshnessDate(currentTime);

            return updateStatus;
        }
    }
}
