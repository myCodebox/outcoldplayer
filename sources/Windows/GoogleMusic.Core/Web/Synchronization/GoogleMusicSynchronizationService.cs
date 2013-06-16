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
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IGoogleMusicSynchronizationService
    {
        Task<SongsUpdateStatus> UpdateSongsAsync();

        Task<UserPlaylistsUpdateStatus> UpdateUserPlaylistsAsync();

        Task<UserPlaylistsUpdateStatus> UpdateUserPlaylistAsync(UserPlaylist userPlaylist);
    }

    public class GoogleMusicSynchronizationService : RepositoryBase, IGoogleMusicSynchronizationService
    {
        private readonly ILogger logger;
        private readonly ISettingsService settingsService;
        private readonly IPlaylistsWebService playlistsWebService;
        private readonly ISongsWebService songsWebService;
        private readonly IUserPlaylistsRepository userPlaylistsRepository;

        public GoogleMusicSynchronizationService(
            ILogManager logManager,
            ISettingsService settingsService,
            IPlaylistsWebService playlistsWebService,
            ISongsWebService songsWebService,
            IUserPlaylistsRepository userPlaylistsRepository)
        {
            this.logger = logManager.CreateLogger("GoogleMusicSynchronizationService");
            this.settingsService = settingsService;
            this.playlistsWebService = playlistsWebService;
            this.songsWebService = songsWebService;
            this.userPlaylistsRepository = userPlaylistsRepository;
        }

        public async Task<SongsUpdateStatus> UpdateSongsAsync()
        {
            int songsUpdated = 0;
            int songsDeleted = 0;
            int songsInstered = 0;

            DateTime? libraryFreshnessDate = this.settingsService.GetLibraryFreshnessDate();
            DateTime currentTime = DateTime.UtcNow;

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("UpdateSongsAsync: streaming load all tracks. Library freshness date {0}.", libraryFreshnessDate);
            }

            var updatedSongs = await this.songsWebService.StreamingLoadAllTracksAsync(libraryFreshnessDate, null);

            if (updatedSongs != null && updatedSongs.Count > 0)
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("UpdateSongsAsync: got {0} updates.", updatedSongs.Count);
                }

                await this.Connection.RunInTransactionAsync(connection =>
                {
                    foreach (var googleSong in updatedSongs)
                    {
                        var songId = googleSong.Id;
                        var storedSong = connection.Find<Song>(x => x.ProviderSongId == songId);

                        if (googleSong.Deleted)
                        {
                            if (storedSong != null)
                            {
                                connection.Delete<Song>(storedSong.SongId);
                            }

                            songsDeleted++;
                        }
                        else
                        {
                            if (storedSong != null)
                            {
                                if (!GoogleMusicSongEx.IsVisualMatch(googleSong, storedSong))
                                {
                                    songsUpdated++;
                                }

                                GoogleMusicSongEx.Mapper(googleSong, storedSong);
                                connection.Update(storedSong);
                            }
                            else
                            {
                                connection.Insert(googleSong.ToSong());
                                songsInstered++;
                            }
                        }
                    }
                });
            }
            else
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("UpdateSongsAsync: no updates.");
                }
            }

            this.settingsService.SetLibraryFreshnessDate(currentTime);

            return new SongsUpdateStatus(songsInstered, songsUpdated, songsDeleted);
        }

        public async Task<UserPlaylistsUpdateStatus> UpdateUserPlaylistsAsync()
        {
            Task<GoogleMusicPlaylists> allUserPlaylistsAsync = this.playlistsWebService.GetAllAsync();
            Task<List<UserPlaylist>> allStoredUserPlaylistsAsync = this.Connection.Table<UserPlaylist>().ToListAsync();

            await Task.WhenAll(allStoredUserPlaylistsAsync, allUserPlaylistsAsync);

            var googlePlaylists = await allUserPlaylistsAsync;
            var existingPlaylists = await allStoredUserPlaylistsAsync;

            if (googlePlaylists.Success.HasValue && !googlePlaylists.Success.Value)
            {
                throw new ApplicationException("PlaylistsWebService:GetAllAsync returns unsuccessful result");
            }

            return await this.UpdateUserPlaylistsInternalAsync(existingPlaylists, googlePlaylists.Playlists ?? Enumerable.Empty<GoogleMusicPlaylist>());
        }

        public async Task<UserPlaylistsUpdateStatus> UpdateUserPlaylistAsync(UserPlaylist userPlaylist)
        {
            GoogleMusicPlaylist googleMusicPlaylist = await this.playlistsWebService.GetAsync(userPlaylist.ProviderPlaylistId);

            return await this.UpdateUserPlaylistsInternalAsync(new[] { userPlaylist }, googleMusicPlaylist == null ? new GoogleMusicPlaylist[] { } : new[] { googleMusicPlaylist });
        }

        private async Task<UserPlaylistsUpdateStatus> UpdateUserPlaylistsInternalAsync(IEnumerable<UserPlaylist> userPlaylists, IEnumerable<GoogleMusicPlaylist> googleMusicPlaylists)
        {
            var existingPlaylists = userPlaylists.ToList();

            int updatedPlaylists = 0;
            int newPlaylists = 0;

            foreach (var googlePlaylist in googleMusicPlaylists)
            {
                bool playlistUpdated = false;

                var providerPlaylistId = googlePlaylist.PlaylistId;

                var userPlaylist = existingPlaylists.FirstOrDefault(x => string.Equals(x.ProviderPlaylistId, providerPlaylistId, StringComparison.OrdinalIgnoreCase));
                if (userPlaylist != null)
                {
                    if (!string.Equals(userPlaylist.Title, googlePlaylist.Title, StringComparison.CurrentCulture))
                    {
                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug(
                                "UpdateUserPlaylistsAsync: title was changed from {0} to {1} (id - {2}).",
                                userPlaylist.Title,
                                googlePlaylist.Title,
                                providerPlaylistId);
                        }

                        userPlaylist.Title = googlePlaylist.Title;
                        userPlaylist.TitleNorm = googlePlaylist.Title.Normalize();
                        playlistUpdated = true;

                        await this.userPlaylistsRepository.UpdateAsync(userPlaylist);
                    }

                    existingPlaylists.Remove(userPlaylist);
                }
                else
                {
                    if (this.logger.IsDebugEnabled)
                    {
                        this.logger.Debug(
                            "UpdateUserPlaylistsAsync: new playlist {0} (id - {1}).",
                            googlePlaylist.Title,
                            providerPlaylistId);
                    }

                    userPlaylist = new UserPlaylist
                    {
                        ProviderPlaylistId = providerPlaylistId,
                        Title = googlePlaylist.Title,
                        TitleNorm = googlePlaylist.Title.Normalize()
                    };

                    await this.userPlaylistsRepository.InsertAsync(userPlaylist);
                    newPlaylists++;
                }

                var userPlaylistSongs = await this.userPlaylistsRepository.GetSongsAsync(userPlaylist.Id, includeAll: true);

                List<UserPlaylistEntry> newEntries = new List<UserPlaylistEntry>();
                List<UserPlaylistEntry> updatedEntries = new List<UserPlaylistEntry>();

                if (googlePlaylist.Playlist != null)
                {
                    for (int songIndex = 0; songIndex < googlePlaylist.Playlist.Count; songIndex++)
                    {
                        var song = googlePlaylist.Playlist[songIndex];
                        var storedSong = userPlaylistSongs.FirstOrDefault(s =>
                                        string.Equals(s.ProviderSongId, song.Id, StringComparison.OrdinalIgnoreCase) &&
                                        string.Equals(s.UserPlaylistEntry.ProviderEntryId, song.PlaylistEntryId, StringComparison.OrdinalIgnoreCase));

                        if (storedSong != null)
                        {
                            if (storedSong.UserPlaylistEntry.PlaylistOrder != songIndex)
                            {
                                if (this.logger.IsDebugEnabled)
                                {
                                    this.logger.Debug(
                                        "UpdateUserPlaylistsAsync: order was changed for entry id {0} (playlist id - {1}).",
                                        storedSong.UserPlaylistEntry.ProviderEntryId,
                                        providerPlaylistId);
                                }

                                storedSong.UserPlaylistEntry.PlaylistOrder = songIndex;
                                updatedEntries.Add(storedSong.UserPlaylistEntry);
                                playlistUpdated = true;
                            }

                            userPlaylistSongs.Remove(storedSong);
                        }
                        else
                        {
                            storedSong = await this.Connection.FindAsync<Song>(x => x.ProviderSongId == song.Id);
                            
                            if (storedSong == null)
                            {
                                storedSong = song.ToSong();
                                storedSong.IsLibrary = false;
                                await this.Connection.InsertAsync(storedSong);
                            }

                            playlistUpdated = true;
                            var entry = new UserPlaylistEntry
                            {
                                PlaylistOrder = songIndex,
                                SongId = storedSong.SongId,
                                ProviderEntryId = song.PlaylistEntryId,
                                PlaylistId = userPlaylist.PlaylistId
                            };

                            newEntries.Add(entry);
                        }
                    }
                }

                if (playlistUpdated)
                {
                    updatedPlaylists++;
                }

                if (newEntries.Count > 0)
                {
                    await this.userPlaylistsRepository.InsertEntriesAsync(newEntries);
                }

                if (updatedEntries.Count > 0)
                {
                    await this.userPlaylistsRepository.UpdateEntriesAsync(updatedEntries);
                }

                if (userPlaylistSongs.Count > 0)
                {
                    await this.userPlaylistsRepository.DeleteEntriesAsync(userPlaylistSongs.Select(entry => entry.UserPlaylistEntry));
                }
            }

            foreach (var existingPlaylist in existingPlaylists)
            {
                await this.userPlaylistsRepository.DeleteAsync(existingPlaylist);
            }

            return new UserPlaylistsUpdateStatus(newPlaylists, updatedPlaylists, existingPlaylists.Count);
        }
    }
}
