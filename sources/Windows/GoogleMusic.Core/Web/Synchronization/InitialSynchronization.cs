// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    
    public interface IInitialSynchronization
    {
        Task InitializeAsync(IProgress<double> progress = null);
    }

    public class InitialSynchronization : IInitialSynchronization
    {
        private const string AllSongsUrl = "services/loadalltracks";

        private readonly ILogger logger;

        private readonly IGoogleMusicWebService googleMusicWebService;
        private readonly ISongsWebService songsWebService;
        private readonly IPlaylistsWebService playlistsWebService;
        private readonly ISongsRepository songsRepository;
        private readonly IUserPlaylistsRepository userPlaylistsRepository;
        private readonly ISettingsService settingsService;
        private readonly INotificationService notificationService;
        private readonly IApplicationResources applicationResources;

        private readonly DbContext dbContext;

        private readonly Dictionary<string, Song> songEntities =
            new Dictionary<string, Song>();
        
        public InitialSynchronization(
            ILogManager logManager,
            IGoogleMusicWebService googleMusicWebService,
            ISongsWebService songsWebService,
            IPlaylistsWebService playlistsWebService,
            ISongsRepository songsRepository,
            IUserPlaylistsRepository userPlaylistsRepository,
            ISettingsService settingsService,
            INotificationService notificationService,
            IApplicationResources applicationResources)
        {
            this.dbContext = new DbContext();
            this.logger = logManager.CreateLogger("InitialSynchronization");
            this.googleMusicWebService = googleMusicWebService;
            this.songsWebService = songsWebService;
            this.playlistsWebService = playlistsWebService;
            this.songsRepository = songsRepository;
            this.userPlaylistsRepository = userPlaylistsRepository;
            this.settingsService = settingsService;
            this.notificationService = notificationService;
            this.applicationResources = applicationResources;
        }

        public async Task InitializeAsync(IProgress<double> progress)
        {
            var statusAsync = this.songsWebService.GetStatusAsync();
            var clearLocalDatabaseAsync = this.ClearLocalDatabaseAsync();

            DateTime currentTime = DateTime.UtcNow;

            this.logger.Debug("InitializeAsync: clear current database and gettings status.");
            await Task.WhenAll(clearLocalDatabaseAsync, statusAsync);
            var status = await statusAsync;
            await progress.SafeReportAsync(0.05d);

            this.logger.Debug("InitializeAsync: loading all songs.");
            var songsProgress = new Progress<int>(async songsCount => await progress.SafeReportAsync((((double)songsCount / status.AvailableTracks) * 0.75d) + 0.05d));
            await this.LoadSongsAsync(songsProgress);

            this.logger.Debug("InitializeAsync: loading all user playlists.");
            var playlistsProgress = new Progress<double>(async plProgress => await progress.SafeReportAsync((plProgress * 0.2d) + 0.8d));

            bool failedToLoadPlaylists = false;

            try
            {
                await this.LoadPlaylistsAsync(playlistsProgress);
            }
            catch (OperationCanceledException exception)
            {
                this.logger.Debug(exception, "Failed to download playlists.");

                failedToLoadPlaylists = true;
            }

            if (failedToLoadPlaylists)
            {
                await this.notificationService.ShowMessageAsync(this.applicationResources.GetString("Notification_FailedToDownloadPlaylists"));
            }

            this.settingsService.SetLibraryFreshnessDate(currentTime);
        }

        private async Task LoadSongsAsync(IProgress<int> progress = null)
        {
            GoogleMusicPlaylist playlist = null;

            Task commitTask = Task.FromResult<object>(null);

            do
            {
                var jsonProperties = new Dictionary<string, string>();

                if (playlist != null && !string.IsNullOrEmpty(playlist.ContinuationToken))
                {
                    jsonProperties.Add("continuationToken", JsonConvert.ToString(playlist.ContinuationToken));
                }

                Task<GoogleMusicPlaylist> loadSongsTask = this.googleMusicWebService.PostAsync<GoogleMusicPlaylist>(AllSongsUrl, jsonProperties: jsonProperties);

                await Task.WhenAll(commitTask, loadSongsTask);

                playlist = await loadSongsTask;

                ICollection<Song> songs;
                if (playlist != null && playlist.Playlist != null)
                {
                    songs = playlist.Playlist.Select(x => x.ToSong()).ToList();
                    this.AddRange(songs);
                }
                else
                {
                    songs = new Song[0];
                }

                commitTask = this.songsRepository.InsertAsync(songs);

                await progress.SafeReportAsync(this.songEntities.Count);
            }
            while (playlist != null && !string.IsNullOrEmpty(playlist.ContinuationToken));

            await commitTask;
        }

        private async Task LoadPlaylistsAsync(IProgress<double> progress)
        {
            await progress.SafeReportAsync(0.0d);

            this.logger.Debug("LoadPlaylistsAsync: loading playlists.");
            var playlists = await this.playlistsWebService.GetAllAsync();

            await progress.SafeReportAsync(0.6d);

            this.logger.Debug("LoadPlaylistsAsync: inserting playlists into database.");
            if (playlists.Playlists != null)
            {
                var entries = new List<UserPlaylistEntry>();

                foreach (var googleUserPlaylist in playlists.Playlists)
                {
                    var userPlaylist = new UserPlaylist
                                           {
                                               ProviderPlaylistId = googleUserPlaylist.PlaylistId,
                                               Title = googleUserPlaylist.Title,
                                               TitleNorm = googleUserPlaylist.Title.Normalize()
                                           };

                    await this.userPlaylistsRepository.InsertAsync(userPlaylist);

                    if (googleUserPlaylist.Playlist != null)
                    {
                        for (int index = 0; index < googleUserPlaylist.Playlist.Count; index++)
                        {
                            GoogleMusicSong googleSong = googleUserPlaylist.Playlist[index];

                            Song song;
                            if (!this.songEntities.TryGetValue(googleSong.Id, out song))
                            {
                                continue;
                            }

                            var entry = new UserPlaylistEntry
                                            {
                                                ProviderEntryId = googleSong.PlaylistEntryId,
                                                PlaylistOrder = index,
                                                SongId = song.SongId,
                                                PlaylistId = userPlaylist.Id
                                            };

                            entries.Add(entry);
                        }
                    }
                }

                await this.userPlaylistsRepository.InsertEntriesAsync(entries);
            }

            await progress.SafeReportAsync(1.0d);
        }

        private async Task ClearLocalDatabaseAsync()
        {
            await this.dbContext.CreateConnection().RunInTransactionAsync(
                c =>
                {
                    c.DeleteAll<UserPlaylist>();
                    c.DeleteAll<UserPlaylistEntry>();
                    c.DeleteAll<Album>();
                    c.DeleteAll<Artist>();
                    c.DeleteAll<Genre>();
                    c.DeleteAll<Song>();
                    c.DeleteAll<CachedSong>();
                    c.DeleteAll<CachedAlbumArt>();
                });
        }

        private void AddRange(IEnumerable<Song> songs)
        {
            foreach (var song in songs)
            {
                this.songEntities[song.ProviderSongId] = song;
            }
        }
    }
}
