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
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.Models;

    using Windows.UI.Xaml;

    public class GoogleMusicSynchronizationService : RepositoryBase, IGoogleMusicSynchronizationService
    {
        private const string LastUpdateKey = "SongsCacheService_CacheFreshnessDate";

        private readonly ILogger logger;

        private readonly ISettingsService settingsService;

        private readonly IPlaylistsWebService playlistsWebService;
        private readonly ISongWebService songWebService;

        private DispatcherTimer dispatcherTimer;

        public GoogleMusicSynchronizationService(
            ILogManager logManager,
            ISettingsService settingsService,
            IPlaylistsWebService playlistsWebService,
            ISongWebService songWebService)
        {
            this.logger = logManager.CreateLogger("GoogleMusicSynchronizationService");
            this.settingsService = settingsService;
            this.playlistsWebService = playlistsWebService;
            this.songWebService = songWebService;
        }

        public async Task InitializeAsync(IProgress<double> progress)
        {
            var lastUpdate = this.settingsService.GetValue<DateTime?>(LastUpdateKey);
            if (lastUpdate == null)
            {
                await this.RefreshAsync(progress);
            }
            else
            {
                await this.SynchronizeAsync(progress);
            }

            this.dispatcherTimer = new DispatcherTimer
                                       {
                                           Interval = TimeSpan.FromMinutes(10)
                                       };
#if NETFX_CORE
            //this.dispatcherTimer.Tick += async (sender, o) =>
            //    {
            //        this.dispatcherTimer.Stop();
            //        try
            //        {
            //            await this.SynchronizeAsync(null);
            //        }
            //        catch (Exception e)
            //        {
            //            this.logger.Error("SynchronizeAsync threw exception in DispatcherTimer.");
            //            this.logger.LogErrorException(e);
            //        }
            //        finally
            //        {
            //            this.dispatcherTimer.Start();
            //        }
            //    };
#endif
            this.dispatcherTimer.Start();
        }

        public async Task RefreshAsync(IProgress<double> progress)
        {
            DateTime lastUpdate = DateTime.UtcNow;

            this.logger.Debug("SynchronizeAsync: clearing local database.");
            await this.ClearLocalDatabaseAsync();

            this.logger.Debug("SynchronizeAsync: getting status.");
            var status = await this.songWebService.GetStatusAsync();

            await progress.SafeReportAsync(0d);

            this.logger.Debug("SynchronizeAsync: loading all songs.");
            var songsProgress = new Progress<int>((songsCount) => progress.Report(((double)songsCount / status.AvailableTracks) * 0.6d));
            var songs = await this.songWebService.GetAllSongsAsync(songsProgress);

            await progress.SafeReportAsync(0.6d);

            var syncContainer = new SongsSyncContainer();

            this.logger.Debug("SynchronizeAsync: insert all songs into database.");

            syncContainer.AddRange(songs);
            
            this.logger.Debug("SynchronizeAsync: loading playlists.");
            var playlists = await this.playlistsWebService.GetAllAsync();

            await progress.SafeReportAsync(0.8d);
            this.logger.Debug("SynchronizeAsync: inserting playlists into database.");

            if (playlists.Playlists != null)
            {
                syncContainer.AddPlaylists(playlists.Playlists);
            }

            await syncContainer.SaveAsync(this.Connection);

            await progress.SafeReportAsync(0.9d);

            this.settingsService.SetValue<DateTime?>(LastUpdateKey, lastUpdate);

            await progress.SafeReportAsync(1d);
        }

        public async Task SynchronizeAsync(IProgress<double> progress)
        {
            var lastUpdate = this.settingsService.GetValue<DateTime?>(LastUpdateKey);

            if (lastUpdate.HasValue)
            {
                DateTime currentTime = DateTime.UtcNow;

                await progress.SafeReportAsync(0d);

                Progress<double> songsProgress = null;
                if (progress != null)
                {
                    songsProgress = new Progress<double>((v) => progress.Report(v * 0.5d));
                }

                await this.SynchronizeSongsAsync(songsProgress, lastUpdate.Value);

                await progress.SafeReportAsync(0.5d);

                Progress<double> userPlaylustsProgress = null;
                if (progress != null)
                {
                    userPlaylustsProgress = new Progress<double>((v) => progress.Report(0.5d + (v * 0.5d)));
                }

                await this.SynchronizeUserPlaylistsAsync(userPlaylustsProgress);

                await progress.SafeReportAsync(1d);

                this.settingsService.SetValue<DateTime?>(LastUpdateKey, currentTime);
            }
            else
            {
                this.logger.Error("SynchronizeAsync: Last Update is null.");
            }
        }

        public async Task ClearLocalDatabaseAsync()
        {
            if (this.dispatcherTimer != null)
            {
                this.dispatcherTimer.Stop();
                this.dispatcherTimer = null;
            }

            this.settingsService.RemoveValue(LastUpdateKey);

            await this.Connection.RunInTransactionAsync(
                (connection) =>
                    {
                        int result = 0;
                        result = connection.DeleteAll<SongEntity>();
                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug("{0} Songs were deleted from DB", result);
                        }

                        result = connection.DeleteAll<UserPlaylistEntity>();
                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug("{0} UserPlaylists were deleted from DB", result);
                        }

                        result = connection.DeleteAll<UserPlaylistEntryEntity>();
                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug("{0} UserPlaylistEntrys were deleted from DB", result);
                        }
                    });
        }

        private async Task SynchronizeSongsAsync(IProgress<double> progress, DateTime lastUpdate)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("SynchronizeSongsAsync: streaming load all tracks.");
            }

            await progress.SafeReportAsync(0d);

            var updatedSongs = await this.songWebService.StreamingLoadAllTracksAsync(lastUpdate, null);

            await progress.SafeReportAsync(0.5d);

            if (updatedSongs.Count > 0)
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("SynchronizeSongsAsync: got {0} updates.", updatedSongs.Count);
                }

                // TODO: Optimization
                //await this.Connection.RunInTransactionAsync(connection =>
                //{
                //    foreach (var song in updatedSongs)
                //    {
                //        var songId = song.Id;

                //        if (song.Deleted)
                //        {
                //            connection.Delete(songId);
                //        }
                //        else
                //        {
                //            var storedSong = connection.Find<SongEntity>(x => x.ProviderSongId == songId);
                //            if (storedSong != null)
                //            {
                //                var songEntity = (SongEntity)song;
                //                songEntity.SongId = storedSong.SongId;
                //                connection.Update(songEntity);
                //            }
                //            else
                //            {
                //                connection.Insert((SongEntity)song);
                //            }
                //        }
                //    }
                //});
            }
            else
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("SynchronizeSongsAsync: no updates.");
                }
            }

            await progress.SafeReportAsync(1d);
        }

        private async Task SynchronizeUserPlaylistsAsync(IProgress<double> progress)
        {
            await progress.SafeReportAsync(0d);

            var googlePlaylists = await this.playlistsWebService.GetAllAsync();
            var existingPlaylists = await this.Connection.Table<UserPlaylistEntity>().ToListAsync();

            await progress.SafeReportAsync(0.4d);

            var pInserts = new List<UserPlaylistEntity>();
            var pUpdates = new List<UserPlaylistEntity>();
            var pDeletes = new List<UserPlaylistEntity>();

            var eInserts = new List<Tuple<UserPlaylistEntity, UserPlaylistEntryEntity>>();
            var eUpdates = new List<Tuple<UserPlaylistEntity, UserPlaylistEntryEntity>>();
            var eDeletes = new List<Tuple<UserPlaylistEntity, UserPlaylistEntryEntity>>();

            int index = 0;

            foreach (var googlePlaylist in googlePlaylists.Playlists ?? Enumerable.Empty<GoogleMusicPlaylist>())
            {
                var providerPlaylistId = googlePlaylist.PlaylistId;

                var userPlaylistEntity = existingPlaylists.FirstOrDefault(x => string.Equals(x.ProviderPlaylistId, providerPlaylistId, StringComparison.Ordinal));
                if (userPlaylistEntity != null)
                {
                    if (!string.Equals(userPlaylistEntity.Title, googlePlaylist.Title, StringComparison.CurrentCulture))
                    {
                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug(
                                "SynchronizeUserPlaylistsAsync: title was changed from {0} to {1} (id - {2}).",
                                userPlaylistEntity.Title,
                                googlePlaylist.Title,
                                providerPlaylistId);
                        }

                        userPlaylistEntity.Title = googlePlaylist.Title;
                        pUpdates.Add(userPlaylistEntity);
                    }

                    existingPlaylists.Remove(userPlaylistEntity);
                }
                else
                {
                    if (this.logger.IsDebugEnabled)
                    {
                        this.logger.Debug(
                            "SynchronizeUserPlaylistsAsync: new playlist {0} (id - {1}).",
                            googlePlaylist.Title,
                            providerPlaylistId);
                    }

                    pInserts.Add(userPlaylistEntity = new UserPlaylistEntity() { ProviderPlaylistId = providerPlaylistId, Title = googlePlaylist.Title });
                }

                var userPlaylistEntries = await this.Connection.Table<UserPlaylistEntryEntity>()
                                                             .Where(e => e.PlaylistId == userPlaylistEntity.PlaylistId)
                                                             .OrderBy(e => e.PlaylistOrder)
                                                             .ToListAsync();

                var userPlaylistSongs = await this.Connection.QueryAsync<SongEntity>(
                        "SELECT s.* FROM Song s INNER JOIN UserPlaylistEntry e ON s.SongId == e.SongId WHERE e.PlaylistId = ?",
                        userPlaylistEntity.PlaylistId);

                for (int songIndex = 0; songIndex < googlePlaylist.Playlist.Count; songIndex++)
                {
                    var song = googlePlaylist.Playlist[songIndex];

                    var storedSong = userPlaylistSongs.FirstOrDefault(s => s.ProviderSongId == song.Id);
                    var entry = storedSong == null ? null : userPlaylistEntries.FirstOrDefault(x => x.SongId == storedSong.SongId);

                    if (entry != null)
                    {
                        if (entry.PlaylistOrder != songIndex)
                        {
                            if (this.logger.IsDebugEnabled)
                            {
                                this.logger.Debug(
                                    "SynchronizeUserPlaylistsAsync: order was changed for entry id {0} (playlist id - {1}).",
                                    entry.ProviderEntryId,
                                    providerPlaylistId);
                            }

                            entry.PlaylistOrder = songIndex;
                            eUpdates.Add(Tuple.Create(userPlaylistEntity, entry));
                        }

                        userPlaylistEntries.Remove(entry);
                    }
                    else
                    {
                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug(
                                "SynchronizeUserPlaylistsAsync: new entry id {0} (playlist id - {1}).",
                                song.PlaylistEntryId,
                                providerPlaylistId);
                        }

                        if (storedSong == null)
                        {
                            storedSong = await this.Connection.FindAsync<SongEntity>(x => x.ProviderSongId == song.Id);
                        }

                        if (storedSong != null)
                        {
                            entry = new UserPlaylistEntryEntity()
                                        {
                                            PlaylistOrder = songIndex,
                                            SongId = storedSong.SongId,
                                            ProviderEntryId = song.PlaylistEntryId
                                        };

                            eInserts.Add(Tuple.Create(userPlaylistEntity, entry));
                        }
                        else
                        {
                            this.logger.Warning("Stored song is null, could not find song for id '{0}' in playlist '{1}'.", song.Id, providerPlaylistId);
                        }
                    }
                }

                eDeletes.AddRange(userPlaylistEntries.Select(x => Tuple.Create(userPlaylistEntity, x)));
                await progress.SafeReportAsync(0.4d + (((double)index++ / googlePlaylists.Playlists.Count) * 0.5d));
            }

            pDeletes.AddRange(existingPlaylists);

            await progress.SafeReportAsync(0.9d);

            await this.Connection.RunInTransactionAsync(
                (connection) =>
                    {
                        foreach (var entry in eDeletes)
                        {
                            connection.Delete<UserPlaylistEntryEntity>(entry);
                        }

                        foreach (var playlist in pDeletes)
                        {
                            connection.Delete<UserPlaylistEntity>(playlist);
                        }

                        if (pUpdates.Count > 0)
                        {
                            connection.UpdateAll(pUpdates);
                        }

                        if (eUpdates.Count > 0)
                        {
                            connection.UpdateAll(eUpdates);
                        }

                        if (pInserts.Count > 0)
                        {
                            connection.InsertAll(pInserts);
                        }

                        if (eInserts.Count > 0)
                        {
                            connection.InsertAll(eInserts.Select(i =>
                                {
                                    i.Item2.PlaylistId = i.Item1.PlaylistId;
                                    return i.Item2;
                                }));
                        }
                    });

            await progress.SafeReportAsync(1d);
        }
    }
}
