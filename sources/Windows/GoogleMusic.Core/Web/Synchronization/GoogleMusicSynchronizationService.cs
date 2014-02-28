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
        Task<Tuple<SongsUpdateStatus, UserPlaylistsUpdateStatus>> Update(IProgress<double> progress = null);
    }

    public class GoogleMusicSynchronizationService : RepositoryBase, IGoogleMusicSynchronizationService
    {
        private readonly ILogger logger;
        private readonly ISettingsService settingsService;
        private readonly IPlaylistsWebService playlistsWebService;
        private readonly ISongsWebService songsWebService;
        private readonly IUserPlaylistsRepository userPlaylistsRepository;
        private readonly ISongsRepository songsRepository;

        public GoogleMusicSynchronizationService(
            ILogManager logManager,
            ISettingsService settingsService,
            IPlaylistsWebService playlistsWebService,
            ISongsWebService songsWebService,
            IUserPlaylistsRepository userPlaylistsRepository,
            ISongsRepository songsRepository)
        {
            this.logger = logManager.CreateLogger("GoogleMusicSynchronizationService");
            this.settingsService = settingsService;
            this.playlistsWebService = playlistsWebService;
            this.songsWebService = songsWebService;
            this.userPlaylistsRepository = userPlaylistsRepository;
            this.songsRepository = songsRepository;
        }

        public async Task<Tuple<SongsUpdateStatus, UserPlaylistsUpdateStatus>> Update(IProgress<double> progress = null)
        {
            DateTime? libraryFreshnessDate = this.settingsService.GetLibraryFreshnessDate();
            DateTime currentTime = DateTime.UtcNow;

            IProgress<int> subProgress = null;

            // Get status if we need to report progress

            if (!libraryFreshnessDate.HasValue && progress != null)
            {
                var status = await this.songsWebService.GetStatusAsync();
                subProgress = new Progress<int>(async songsCount => await progress.SafeReportAsync((((double)songsCount / status.AvailableTracks) * 0.75d) + 0.05d));
            }

            await progress.SafeReportAsync(0.05d);

            int songsUpdated = 0;
            int songsDeleted = 0;
            int songsInstered = 0;

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
                            songsDeleted++;
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
                                    songsUpdated++;
                                }
                            }
                            else
                            {
                                toBeInserted.Add(googleMusicSong.ToSong());
                                songsInstered++;
                            }
                        }
                    }

                    if (toBeDeleted.Count > 0)
                    {
                        await this.songsRepository.DeleteAsync(toBeDeleted);
                    }

                    if (toBeInserted.Count > 0)
                    {
                        await this.songsRepository.InsertAsync(toBeInserted);
                    }

                    if (toBeUpdated.Count > 0)
                    {
                        await this.songsRepository.UpdateAsync(toBeUpdated);
                    }
                });

            await progress.SafeReportAsync(0.8d);

            int playlistsInserted = 0;
            int playlistsUpdated = 0;
            int playlistsDeleted = 0;

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
                        playlistsDeleted++;
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
                            playlistsInserted++;
                        }
                    }
                }

                if (toBeDeleted.Count > 0)
                {
                    await this.userPlaylistsRepository.DeleteAsync(toBeDeleted);
                }

                if (toBeInserted.Count > 0)
                {
                    await this.userPlaylistsRepository.InsertAsync(toBeInserted);
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
                    playlistsUpdated++;

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
                            playlistsInserted++;
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
                    await this.userPlaylistsRepository.DeleteEntriesAsync(toBeDeleted);
                }

                if (toBeInserted.Count > 0)
                {
                    await this.userPlaylistsRepository.InsertEntriesAsync(toBeInserted);
                }

                if (toBeUpdated.Count > 0)
                {
                    await this.userPlaylistsRepository.UpdateEntriesAsync(toBeUpdated);
                }
            });

            await progress.SafeReportAsync(1.0d);
            this.settingsService.SetLibraryFreshnessDate(currentTime);

            return Tuple.Create(new SongsUpdateStatus(songsInstered, songsUpdated, songsDeleted), new UserPlaylistsUpdateStatus(playlistsInserted, playlistsUpdated, playlistsDeleted));
        }
    }
}
