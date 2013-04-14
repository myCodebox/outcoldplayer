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

                        if (googleSong.Deleted)
                        {
                            connection.Delete(songId);
                            songsDeleted++;
                        }
                        else
                        {
                            var storedSong = connection.Find<Song>(x => x.ProviderSongId == songId);
                            
                            var songEntity = googleSong.ToSong();
                            if (storedSong != null)
                            {
                                songEntity.SongId = storedSong.SongId;
                                connection.Update(songEntity);
                                songsUpdated++;
                            }
                            else
                            {
                                connection.Insert(songEntity);
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

                    await this.userPlaylistsRepository.InstertAsync(userPlaylist);
                    newPlaylists++;
                }

                var userPlaylistSongs = await this.userPlaylistsRepository.GetSongsAsync(userPlaylist.Id);

                List<UserPlaylistEntry> newEntries = new List<UserPlaylistEntry>();
                List<UserPlaylistEntry> updatedEntries = new List<UserPlaylistEntry>();

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
                        if (storedSong != null)
                        {
                            playlistUpdated = true;
                            var entry = new UserPlaylistEntry
                            {
                                PlaylistOrder = songIndex,
                                SongId = storedSong.SongId,
                                ProviderEntryId = song.PlaylistEntryId,
                                PlaylistId = userPlaylist.Id
                            };

                            newEntries.Add(entry);
                        }
                        else
                        {
                            this.logger.Warning("Stored song is null, could not find song for id '{0}' in playlist '{1}'.", song.Id, providerPlaylistId);
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
