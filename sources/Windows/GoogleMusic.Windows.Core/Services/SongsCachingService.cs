// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.EventAggregator;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    using Windows.Storage;
    using Windows.Storage.Streams;

    public class SongsCachingService : ISongsCachingService
    {
        private const string SongsCacheFolder = "SongsCache";
        private const string SongsMusicLibraryCacheFolder = ".outcoldplayer.cache";

        private readonly ISongsWebService songsWebService;
        private readonly ICachedSongsRepository songsCacheRepository;
        private readonly ISongsRepository songsRepository;
        private readonly IMediaStreamDownloadService mediaStreamDownloadService;
        private readonly IAlbumArtCacheService albumArtCacheService;
        private readonly IApplicationStateService stateService;
        private readonly IEventAggregator eventAggregator;
        private readonly IApplicationResources resources;
        private readonly INotificationService notificationService;
        private readonly ISettingsService settingsService;
        private readonly ILogger logger;

        private readonly SemaphoreSlim mutex = new SemaphoreSlim(1);

        private StorageFolder cacheFolder;

        private INetworkRandomAccessStream currentDownloadStream;
        private INetworkRandomAccessStream preDownloadedStream;
        private Song currentDownloadSong;
        private Song preDownloadedSong;

        private Task downloadTask;
        private CancellationTokenSource downloadTaskCancellationToken;

        public SongsCachingService(
            ILogManager logManager,
            ISongsWebService songsWebService,
            ICachedSongsRepository songsCacheRepository,
            ISongsRepository songsRepository,
            IMediaStreamDownloadService mediaStreamDownloadService,
            IAlbumArtCacheService albumArtCacheService,
            IApplicationStateService stateService,
            IEventAggregator eventAggregator,
            IApplicationResources resources,
            INotificationService notificationService,
            ISettingsService settingsService)
        {
            this.logger = logManager.CreateLogger("SongsCachingService");
            this.songsWebService = songsWebService;
            this.songsCacheRepository = songsCacheRepository;
            this.songsRepository = songsRepository;
            this.mediaStreamDownloadService = mediaStreamDownloadService;
            this.albumArtCacheService = albumArtCacheService;
            this.stateService = stateService;
            this.eventAggregator = eventAggregator;
            this.resources = resources;
            this.notificationService = notificationService;
            this.settingsService = settingsService;

            this.eventAggregator.GetEvent<SettingsChangeEvent>()
                .Where(x => string.Equals(x.Key, SettingsServiceExtensions.IsMusicLibraryForCacheKey, StringComparison.OrdinalIgnoreCase))
                .Subscribe(async (e) =>
                {
                    await this.mutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
                    this.cacheFolder = null;
                    this.mutex.Release(1);
                });

            this.eventAggregator.GetEvent<ApplicationStateChangeEvent>()
                .Subscribe(async (e) =>
                {
                    if (e.CurrentState == ApplicationState.Offline)
                    {
                        await this.CancelDownloadTaskAsync();
                    }
                    else if (e.CurrentState == ApplicationState.Online)
                    {
                        this.StartDownloadTask();
                    }
                });
        }

        public async Task<IStream> GetStreamAsync(Song song, CancellationToken token)
        {
            if (song == null)
            {
                throw new ArgumentNullException("song");
            }

            await this.mutex.WaitAsync(token).ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                var cache = await this.songsCacheRepository.FindAsync(song);
                if (cache != null && !string.IsNullOrEmpty(cache.FileName))
                {
                    // TODO: Catch exception if file does not exist
                    var storageFile = await this.GetFileAsync(cache.FileName);
                    return await this.mediaStreamDownloadService.GetCachedStreamAsync(new WindowsStorageFile(storageFile), token);
                }

                if (this.preDownloadedSong != null && this.preDownloadedStream != null)
                {
                    try
                    {
                        if (string.Equals(this.preDownloadedSong.SongId, song.SongId, StringComparison.Ordinal) && !this.preDownloadedStream.IsFailed)
                        {
                            return this.preDownloadedStream;
                        }
                    }
                    finally 
                    {
                        this.preDownloadedSong = null;
                        this.preDownloadedStream = null;
                    }
                }

                if (this.currentDownloadSong != null && this.currentDownloadStream != null)
                {
                    try
                    {
                        if (string.Equals(this.currentDownloadSong.SongId, song.SongId, StringComparison.Ordinal) && !this.currentDownloadStream.IsFailed)
                        {
                            return this.currentDownloadStream;
                        }
                    }
                    finally
                    {
                        this.currentDownloadSong = null;
                        this.currentDownloadStream = null;
                    }
                }
            }
            finally
            {
                this.mutex.Release(1);
            }

            if (this.stateService.IsOffline())
            {
                return null;
            }

            await this.CancelDownloadTaskAsync();

            var result = await this.GetNetworkStreamAsync(song, token);

            if (token.IsCancellationRequested)
            {
                return null;
            }

            INetworkRandomAccessStream networkRandomAccessStream = result.Item1;

            if (result.Item1 == null)
            {
                if (result.Item2 == HttpStatusCode.InternalServerError
                    || result.Item2 == HttpStatusCode.ServiceUnavailable)
                {
                    this.logger.LogTask(this.notificationService.ShowMessageAsync("Something bad happened on the Google Service. "
                                                                                  + "Try again."));
                }
                else if ((int)result.Item2 >= 500)
                {
                    this.logger.LogTask(this.notificationService.ShowMessageAsync("Could not load current song. "
                                                                                  + "Verify network connection and try again."));
                }
                else if (song.IsDeleted() || result.Item2 == HttpStatusCode.NotFound)
                {
                    this.logger.LogTask(this.notificationService.ShowMessageAsync("It looks like that this songs has been removed from All Access library. "
                                                                                  + "You may try to search for this song in different albums."));
                }
                else
                {
                    if (!song.IsLibrary)
                    {
                        this.logger.LogTask(this.notificationService.ShowMessageAsync(this.resources.GetString("Msg_AllAccessDisabled")));
                    }
                    else
                    {
                        this.logger.LogTask(this.notificationService.ShowMessageAsync(this.resources.GetString("Player_CannotPlay")));
                    }
                }
            }

            await this.SetCurrentStreamAsync(song, networkRandomAccessStream);

            return networkRandomAccessStream;
        }

        public async Task PredownloadStreamAsync(Song song, CancellationToken token)
        {
            if (song == null)
            {
                throw new ArgumentNullException("song");
            }

            await this.mutex.WaitAsync(token).ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                var cache = await this.songsCacheRepository.FindAsync(song);
                if (cache != null && !string.IsNullOrEmpty(cache.FileName))
                {
                    return;
                }

                if (this.preDownloadedSong != null && string.Equals(this.preDownloadedSong.SongId, song.SongId, StringComparison.Ordinal)
                    && this.preDownloadedStream != null && !this.preDownloadedStream.IsFailed)
                {
                    return;
                }

                if (this.currentDownloadSong != null && string.Equals(this.currentDownloadSong.SongId, song.SongId, StringComparison.Ordinal)
                     && this.currentDownloadStream != null && !this.currentDownloadStream.IsFailed)
                {
                    return;
                }
            }
            finally
            {
                this.mutex.Release(1);
            }

            if (this.stateService.IsOffline())
            {
                return;
            }

            await this.CancelDownloadTaskAsync();

            INetworkRandomAccessStream networkRandomAccessStream = (await this.GetNetworkStreamAsync(song, token)).Item1;

            await this.SetCurrentStreamAsync(song, networkRandomAccessStream);

            if (networkRandomAccessStream != null)
            {
                try
                {
                    await networkRandomAccessStream.DownloadAsync();
                    await this.mutex.WaitAsync(token).ConfigureAwait(continueOnCapturedContext: false);

                    try
                    {
                        this.preDownloadedSong = song;
                        this.preDownloadedStream = networkRandomAccessStream;
                    }
                    finally
                    {
                        this.mutex.Release(1);
                    }
                }
                catch (Exception exception)
                {
                    if (exception is TaskCanceledException)
                    {
                        this.logger.Debug("PredownloadStreamAsync was cancelled.");
                    }
                    else
                    {
                        this.logger.Error(exception, "Exception while tried to PredownloadStreamAsync.");
                    }
                }
            }

            await this.StartDownloadTaskAsync();
        }
        
        public async Task QueueForDownloadAsync(IEnumerable<Song> songs)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            foreach (var song in songs)
            {
                await this.songsCacheRepository.AddAsync(
                        new CachedSong
                            {
                                SongId = song.SongId, 
                                TaskAdded = DateTime.UtcNow,
                                IsAddedByUser = true
                            });
            }

            await this.StartDownloadTaskAsync();
        }

        public async Task<IFolder> GetCacheFolderAsync()
        {
            await this.InitializeCacheFolderAsync();
            return new WindowsStorageFolder(this.cacheFolder);
        }

        public async Task ClearCacheAsync()
        {
            Song currentSong = null;
            INetworkRandomAccessStream currentStream = null;

            await this.mutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                if (this.downloadTask != null)
                {
                    if (this.downloadTaskCancellationToken != null)
                    {
                        this.downloadTaskCancellationToken.Cancel();
                        this.downloadTaskCancellationToken = null;
                    }

                    if (this.downloadTask != null && this.currentDownloadSong != null && this.currentDownloadStream != null)
                    {
                        currentSong = this.currentDownloadSong;
                        currentStream = this.currentDownloadStream;
                    }

                    if (this.currentDownloadStream != null)
                    {
                        this.currentDownloadStream.Dispose();
                    }

                    this.downloadTask = null;
                    this.currentDownloadSong = null;
                    this.currentDownloadStream = null;
                }
            }
            finally
            {
                this.mutex.Release(1);
            }

            if (currentSong != null && currentStream != null)
            {
                this.eventAggregator.Publish(
                    new SongCachingChangeEvent(SongCachingChangeEventType.DownloadCanceled, currentStream, currentSong));
            }

            await this.InitializeCacheFolderAsync();
            await this.songsCacheRepository.ClearCacheAsync();
            foreach (var storageItem in await this.cacheFolder.GetItemsAsync())
            {
                await storageItem.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }

            this.eventAggregator.Publish(new CachingChangeEvent(SongCachingChangeEventType.ClearCache));
        }

        public async Task ClearCachedAsync(IEnumerable<Song> songs)
        {
            await this.CancelDownloadTaskAsync();
            await this.InitializeCacheFolderAsync();

            foreach (var song in songs)
            {
                var cache = await this.songsCacheRepository.FindAsync(song);
                if (cache != null)
                {
                    await this.songsCacheRepository.RemoveAsync(cache);
                    var file = await this.GetFileAsync(cache.FileName);
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);

                    this.eventAggregator.Publish(new SongCachingChangeEvent(SongCachingChangeEventType.RemoveLocalCopy, null, song));
                }
            }

            this.StartDownloadTask();
        }

        public async Task CancelTaskAsync(CachedSong cachedSong)
        {
            if (cachedSong == null)
            {
                throw new ArgumentNullException("cachedSong");
            }

            Song currentSong = null;
            INetworkRandomAccessStream currentStream = null;

            await this.mutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                if (this.downloadTask != null && this.currentDownloadSong != null && this.currentDownloadSong.SongId == cachedSong.SongId)
                {
                    if (this.downloadTaskCancellationToken != null)
                    {
                        this.downloadTaskCancellationToken.Cancel();
                        this.downloadTaskCancellationToken = null;
                    }

                    if (this.downloadTask != null && this.currentDownloadSong != null && this.currentDownloadStream != null)
                    {
                        currentSong = this.currentDownloadSong;
                        currentStream = this.currentDownloadStream;
                    }

                    if (this.currentDownloadStream != null)
                    {
                        this.currentDownloadStream.Dispose();
                    }

                    this.downloadTask = null;
                    this.currentDownloadSong = null;
                    this.currentDownloadStream = null;
                }

                var refreshedCache = await this.songsCacheRepository.FindAsync(cachedSong.Song);
                if (refreshedCache != null)
                {
                    if (!string.IsNullOrEmpty(refreshedCache.FileName))
                    {
                        var storageFile = await this.GetFileAsync(refreshedCache.FileName);
                        await storageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }

                    await this.songsCacheRepository.RemoveAsync(refreshedCache);
                }
            }
            finally
            {
                this.mutex.Release(1);
            }

            if (currentSong != null && currentStream != null)
            {
                this.eventAggregator.Publish(
                    new SongCachingChangeEvent(SongCachingChangeEventType.DownloadCanceled, currentStream, currentSong));
            }

            await this.StartDownloadTaskAsync();
        }

        public Task<IList<CachedSong>> GetAllActiveTasksAsync()
        {
            return this.songsCacheRepository.GetAllQueuedTasksAsync();
        }

        public async Task<Tuple<INetworkRandomAccessStream, Song>> GetCurrentTaskAsync()
        {
            await this.mutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                if (this.currentDownloadSong != null && this.currentDownloadStream != null && this.downloadTask != null)
                {
                    return Tuple.Create(this.currentDownloadStream, this.currentDownloadSong);
                }
            }
            finally
            {
                this.mutex.Release(1);
            }

            return null;
        }

        public async void StartDownloadTask()
        {
            await this.StartDownloadTaskAsync();
        }

        public bool IsDownloading()
        {
            this.mutex.Wait();

            try
            {
                return this.downloadTask != null;
            }
            finally
            {
                this.mutex.Release(1);
            }
        }

        public async Task RestoreCacheAsync()
        {
            StorageFolder storageFolder = ((WindowsStorageFolder)(await this.GetCacheFolderAsync())).Folder;
            var folders = await storageFolder.GetFoldersAsync();
            foreach (var folder in folders)
            {
                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                foreach (var file in files)
                {
                    var song = await this.songsRepository.FindSongAsync(file.Name);
                    if (song != null)
                    {
                        CachedSong cachedSong = new CachedSong()
                        {
                            FileName = file.Name,
                            SongId = song.SongId,
                            IsAddedByUser = true,
                            TaskAdded = DateTime.UtcNow
                        };

                        await this.songsCacheRepository.AddAsync(cachedSong);
                    }
                    else
                    {
                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                }
            }
        }

        public async Task<IFolder> GetAppDataStorageFolderAsync()
        {
            return new WindowsStorageFolder(await ApplicationData.Current.LocalFolder.CreateFolderAsync(SongsCacheFolder, CreationCollisionOption.OpenIfExists));
        }

        public async Task<IFolder> GetMusicLibraryStorageFolderAsync()
        {
            return new WindowsStorageFolder(await KnownFolders.MusicLibrary.CreateFolderAsync(SongsMusicLibraryCacheFolder, CreationCollisionOption.OpenIfExists));
        }

        private async Task SetCurrentStreamAsync(Song song, INetworkRandomAccessStream networkRandomAccessStream)
        {
            await this.mutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                this.currentDownloadStream = networkRandomAccessStream;
                this.currentDownloadSong = song;
            }
            finally
            {
                this.mutex.Release(1);
            }
        }

        private async Task<Tuple<INetworkRandomAccessStream, HttpStatusCode>> GetNetworkStreamAsync(Song song, CancellationToken token)
        {
            GoogleMusicSongUrl songUrl = null;

            HttpStatusCode statusCode = HttpStatusCode.OK;

            try
            {
                songUrl = await this.songsWebService.GetSongUrlAsync(song, token);
            }
            catch (WebRequestException e)
            {
                statusCode = e.StatusCode;

                if (e.StatusCode == HttpStatusCode.Forbidden)
                {
                    this.logger.Debug("Forbidden: Exception while tried to get song url: {0}", e);
                }
                else if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    this.logger.Debug("Not Found: Exception while tried to get song url: {0}", e);
                }
                else
                {
                    this.logger.Error(
                        new WebRequestException(string.Format("Cannot get network stream - {0}.", e.StatusCode), e.InnerException, e.StatusCode), 
                        "Exception while tried to get song url.");
                }
            }
            catch (Exception e)
            {
                this.logger.Error(e, "Exception while tried to get song url.");
            }

            INetworkRandomAccessStream stream = null;

            if (songUrl != null)
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Getting stream by url '{0}'.", songUrl.Url);
                }
                
                try
                {
                    if (string.IsNullOrEmpty(songUrl.Url))
                    {
                        stream = await this.mediaStreamDownloadService.GetStreamAsync(songUrl.Urls, token);
                    }
                    else
                    {
                        stream = await this.mediaStreamDownloadService.GetStreamAsync(songUrl.Url, token);
                    }
                }
                catch (Exception exception)
                {
                    if (exception is TaskCanceledException)
                    {
                        this.logger.Debug("GetStreamAsync was cancelled.");
                    }
                    else
                    {
                        this.logger.Error(exception, "Exception while tried to get stream.");
                    }
                }
            }

            return new Tuple<INetworkRandomAccessStream, HttpStatusCode>(stream, (stream == null && statusCode == HttpStatusCode.OK) ? HttpStatusCode.GatewayTimeout : statusCode);
        }

        private async Task StartDownloadTaskAsync()
        {
            if (this.stateService.IsOffline())
            {
                return;
            }

            await this.mutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                if (this.downloadTask == null)
                {
                    this.downloadTaskCancellationToken = new CancellationTokenSource();
                    this.downloadTask = this.DownloadAsync(this.downloadTaskCancellationToken.Token);
                }
            }
            finally
            {
                this.mutex.Release(1);
            }
        }

        public async Task CancelDownloadTaskAsync()
        {
            Song currentSong = null;
            INetworkRandomAccessStream currentStream = null;

            await this.mutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                if (this.downloadTaskCancellationToken != null)
                {
                    this.downloadTaskCancellationToken.Cancel();
                    this.downloadTaskCancellationToken = null;
                }

                if (this.downloadTask != null && this.currentDownloadSong != null && this.currentDownloadStream != null)
                {
                    currentSong = this.currentDownloadSong;
                    currentStream = this.currentDownloadStream;
                }

                this.downloadTask = null;

                if (this.currentDownloadStream != null)
                {
                    this.currentDownloadStream.Dispose();
                }

                this.currentDownloadSong = null;
                this.currentDownloadStream = null;

                this.preDownloadedSong = null;
                this.preDownloadedStream = null;
            }
            finally
            {
                this.mutex.Release(1);
            }

            if (currentSong != null && currentStream != null)
            {
                this.eventAggregator.Publish(
                    new SongCachingChangeEvent(SongCachingChangeEventType.DownloadCanceled, currentStream, currentSong));
            }
        }

        private async Task DownloadAsync(CancellationToken cancellationToken)
        {
            try
            {
                await this.InitializeCacheFolderAsync();

                CachedSong nextTask;
                while ((nextTask = await this.songsCacheRepository.GetNextAsync()) != null)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    INetworkRandomAccessStream stream = null;

                    await this.mutex.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                    try
                    {
                        if (this.currentDownloadSong != null && this.currentDownloadSong.SongId == nextTask.SongId)
                        {
                            stream = this.currentDownloadStream;
                        }

                        if (this.preDownloadedSong != null && this.preDownloadedSong.SongId == nextTask.SongId)
                        {
                            stream = this.preDownloadedStream;
                        }
                    }
                    finally
                    {
                        this.mutex.Release(1);
                    }

                    if (stream == null)
                    {
                        stream = (await this.GetNetworkStreamAsync(nextTask.Song, cancellationToken)).Item1;
                    }

                    this.eventAggregator.Publish(new SongCachingChangeEvent(SongCachingChangeEventType.StartDownloading, stream, nextTask.Song));

                    if (stream == null)
                    {
                        await this.ClearDownloadTask(cancellationToken);
                        this.eventAggregator.Publish(new SongCachingChangeEvent(SongCachingChangeEventType.FailedToDownload, null, nextTask.Song));
                        break;
                    }
                    else
                    {
                        await this.SetCurrentStreamAsync(nextTask.Song, stream);
                        await this.InitializeCacheFolderAsync();
                        await stream.DownloadAsync();

                        if (nextTask.Song.AlbumArtUrl != null)
                        {
                            await this.albumArtCacheService.GetCachedImageAsync(nextTask.Song.AlbumArtUrl, size: 79);
                            await this.albumArtCacheService.GetCachedImageAsync(nextTask.Song.AlbumArtUrl, size: 160);
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        await this.mutex.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                        try
                        {
                            var cache = await this.songsCacheRepository.FindAsync(nextTask.Song);
                            if (cache == null || string.IsNullOrEmpty(cache.FileName))
                            {
                                var songFolder = await this.cacheFolder.CreateFolderAsync(nextTask.Song.SongId.Substring(0, 1), CreationCollisionOption.OpenIfExists);
                                var file = await songFolder.CreateFileAsync(nextTask.Song.SongId, CreationCollisionOption.ReplaceExisting);
                                await stream.SaveToFileAsync(new WindowsStorageFile(file));

                                if (cache == null)
                                {
                                    cache = new CachedSong() { FileName = file.Name, SongId = nextTask.Song.SongId, TaskAdded = DateTime.UtcNow };
                                    await this.songsCacheRepository.AddAsync(cache);
                                }
                                else
                                {
                                    cache.FileName = file.Name;
                                    await this.songsCacheRepository.UpdateAsync(cache);
                                }
                            }

                            this.currentDownloadStream = null;
                            this.currentDownloadSong = null;
                        }
                        finally
                        {
                            this.mutex.Release(1);
                        }

                        var song = await this.songsRepository.GetSongAsync(nextTask.Song.SongId);
                        this.eventAggregator.Publish(new SongCachingChangeEvent(SongCachingChangeEventType.FinishDownloading, stream, song));
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                this.logger.Debug(e, "DownloadAsync was canceled.");
            }
            catch (Exception exception)
            {
                this.logger.Error(exception, "Exception while tried to DownloadAsync.");
            }

            await this.ClearDownloadTask(cancellationToken);
        }

        private async Task ClearDownloadTask(CancellationToken cancellationToken)
        {
            await this.mutex.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                this.downloadTask = null;
                this.downloadTaskCancellationToken = null;
            }
            finally
            {
                this.mutex.Release(1);
            }
        }

        private async Task InitializeCacheFolderAsync()
        {
            await this.mutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                if (this.cacheFolder == null)
                {
                    if (this.settingsService.GetIsMusicLibraryForCache())
                    {
                        this.cacheFolder = await KnownFolders.MusicLibrary.CreateFolderAsync(SongsMusicLibraryCacheFolder, CreationCollisionOption.OpenIfExists);
                    }
                    else
                    {
                        var localFolder = ApplicationData.Current.LocalFolder;
                        this.cacheFolder = await localFolder.CreateFolderAsync(SongsCacheFolder, CreationCollisionOption.OpenIfExists);
                    }
                }
            }
            finally
            {
                this.mutex.Release(1);
            }
        }

        private async Task<StorageFile> GetFileAsync(string fileName)
        {
            StorageFolder folder = await this.cacheFolder.GetFolderAsync(fileName.Substring(0, 1));
            return await folder.GetFileAsync(fileName);
        }

        private async void HandleGetStreamFinished(INetworkRandomAccessStream networkRandomAccessStream)
        {
            try
            {
                if (networkRandomAccessStream != null)
                {
                    await networkRandomAccessStream.DownloadAsync();
                }
            }
            catch (Exception exception)
            {
                if (exception is TaskCanceledException)
                {
                    this.logger.Debug("HandleGetStreamFinished was canceled.");
                }
                else
                {
                    this.logger.Error(exception, "Exception while tried to HandleGetStreamFinished.");
                }
            }

            await this.StartDownloadTaskAsync();
        }
    }
}
