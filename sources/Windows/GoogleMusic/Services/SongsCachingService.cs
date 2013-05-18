// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    using Windows.Storage;
    using Windows.Storage.Streams;

    public enum SongCachingChangeEventType
    {
        Unknown = 1,
        StartDownloading = 2,
        FinishDownloading = 3,
        FailedToDownload = 4,
        DownloadCanceled = 5,
        ClearCache = 6
    }

    public interface ISongsCachingService
    {
        Task<IRandomAccessStream> GetStreamAsync(Song song);

        Task PredownloadStreamAsync(Song song);

        Task QueueForDownloadAsync(IEnumerable<Song> song);

        Task<StorageFolder> GetCacheFolderAsync();

        Task ClearCacheAsync();

        Task CancelTaskAsync(CachedSong cachedSong);

        Task<IList<CachedSong>> GetAllActiveTasksAsync();

        Task<Tuple<INetworkRandomAccessStream, Song>> GetCurrentTaskAsync();

        void StartDownloadTask();

        bool IsDownloading();
    }

    public class CachingChangeEvent
    {
        public CachingChangeEvent(SongCachingChangeEventType eventType)
        {
            this.EventType = eventType;
        }

        public SongCachingChangeEventType EventType { get; private set; }
    }

    public class SongCachingChangeEvent : CachingChangeEvent
    {
        public SongCachingChangeEvent(
            SongCachingChangeEventType eventType, 
            INetworkRandomAccessStream stream, 
            Song song)
            : base(eventType)
        {
            this.Stream = stream;
            this.Song = song;
        }

        public INetworkRandomAccessStream Stream { get; private set; }

        public Song Song { get; private set; }
    }

    internal class SongsCachingService : ISongsCachingService
    {
        private const string SongsCacheFolder = "SongsCache";

        private readonly ISongsWebService songsWebService;
        private readonly ICachedSongsRepository songsCacheRepository;
        private readonly ISongsRepository songsRepository;
        private readonly IMediaStreamDownloadService mediaStreamDownloadService;
        private readonly IAlbumArtCacheService albumArtCacheService;

        private readonly IApplicationStateService stateService;

        private readonly IEventAggregator eventAggregator;
        private readonly ILogger logger;

        private readonly SemaphoreSlim mutex = new SemaphoreSlim(1);

        private StorageFolder cacheFolder;

        private INetworkRandomAccessStream currentDownloadStream;
        private INetworkRandomAccessStream predownloadedStream;
        private Song currentDownloadSong;
        private Song predownloadedSong;

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
            IEventAggregator eventAggregator)
        {
            this.logger = logManager.CreateLogger("SongsCachingService");
            this.songsWebService = songsWebService;
            this.songsCacheRepository = songsCacheRepository;
            this.songsRepository = songsRepository;
            this.mediaStreamDownloadService = mediaStreamDownloadService;
            this.albumArtCacheService = albumArtCacheService;
            this.stateService = stateService;
            this.eventAggregator = eventAggregator;

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

        public async Task<IRandomAccessStream> GetStreamAsync(Song song)
        {
            if (song == null)
            {
                throw new ArgumentNullException("song");
            }

            await this.mutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                var cache = await this.songsCacheRepository.FindAsync(song);
                if (cache != null && !string.IsNullOrEmpty(cache.FileName))
                {
                    // TODO: Catch exception if file does not exist
                    var storageFile = await StorageFile.GetFileFromPathAsync(this.GetFullPath(cache.FileName));
                    return await this.mediaStreamDownloadService.GetCachedStreamAsync(storageFile);
                }

                if (this.predownloadedSong != null && this.predownloadedSong.SongId == song.SongId)
                {
                    return this.predownloadedStream;
                }

                if (this.currentDownloadSong != null && this.currentDownloadSong.SongId == song.SongId)
                {
                    return this.currentDownloadStream;
                }
            }
            finally
            {
                this.mutex.Release(1);
            }

            await this.CancelDownloadTaskAsync();

            INetworkRandomAccessStream networkRandomAccessStream = await this.GetNetworkStreamAsync(song);

            await this.SetCurrentStreamAsync(song, networkRandomAccessStream);

            this.HandleGetStreamFinished(networkRandomAccessStream);

            return networkRandomAccessStream;
        }

        public async Task PredownloadStreamAsync(Song song)
        {
            if (song == null)
            {
                throw new ArgumentNullException("song");
            }

            await this.mutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                var cache = await this.songsCacheRepository.FindAsync(song);
                if (cache != null && !string.IsNullOrEmpty(cache.FileName))
                {
                    return;
                }

                if (this.predownloadedSong != null && this.predownloadedSong.SongId == song.SongId)
                {
                    return;
                }

                if (this.currentDownloadSong != null && this.currentDownloadSong.SongId == song.SongId)
                {
                    return;
                }
            }
            finally
            {
                this.mutex.Release(1);
            }

            await this.CancelDownloadTaskAsync();

            INetworkRandomAccessStream networkRandomAccessStream = await this.GetNetworkStreamAsync(song);

            await this.SetCurrentStreamAsync(song, networkRandomAccessStream);

            if (networkRandomAccessStream != null)
            {
                try
                {
                    await networkRandomAccessStream.DownloadAsync();
                    await this.mutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

                    try
                    {
                        this.predownloadedSong = song;
                        this.predownloadedStream = networkRandomAccessStream;
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
                                TaskAdded = DateTime.Now,
                                IsAddedByUser = true
                            });
            }

            await this.StartDownloadTaskAsync();
        }

        public async Task<StorageFolder> GetCacheFolderAsync()
        {
            await this.InitializeCacheFolderAsync();
            return this.cacheFolder;
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
                if (!string.IsNullOrEmpty(refreshedCache.FileName))
                {
                    var storageFile = await StorageFile.GetFileFromPathAsync(this.GetFullPath(refreshedCache.FileName));
                    await storageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }

                await this.songsCacheRepository.RemoveAsync(refreshedCache);
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

        public async Task<Tuple<INetworkRandomAccessStream, Song>>  GetCurrentTaskAsync()
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

        private async Task<INetworkRandomAccessStream> GetNetworkStreamAsync(Song song)
        {
            GoogleMusicSongUrl songUrl = null;

            try
            {
                songUrl = await this.songsWebService.GetSongUrlAsync(song.ProviderSongId);
            }
            catch (WebRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.Forbidden)
                {
                    this.logger.Debug("Exception while tried to get song url: {0}", e);
                }
                else
                {
                    this.logger.Error(e, "Exception while tried to get song url.");
                }
            }
            catch (Exception e)
            {
                this.logger.Error(e, "Exception while tried to get song url.");
            }

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
                        return await this.mediaStreamDownloadService.GetStreamAsync(songUrl.Urls);
                    }
                    else
                    {
                        return await this.mediaStreamDownloadService.GetStreamAsync(songUrl.Url);
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

            return null;
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

        private async Task CancelDownloadTaskAsync()
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

                this.predownloadedSong = null;
                this.predownloadedStream = null;
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

                        if (this.predownloadedSong != null && this.predownloadedSong.SongId == nextTask.SongId)
                        {
                            stream = this.predownloadedStream;
                        }
                    }
                    finally
                    {
                        this.mutex.Release(1);
                    }

                    if (stream == null)
                    {
                        stream = await this.GetNetworkStreamAsync(nextTask.Song);
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
                            await this.albumArtCacheService.GetCachedImageAsync(nextTask.Song.AlbumArtUrl, size: 116);
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
                                var songFolder = await this.cacheFolder.CreateFolderAsync(nextTask.Song.ProviderSongId.Substring(0, 1), CreationCollisionOption.OpenIfExists);
                                var file = await songFolder.CreateFileAsync(nextTask.Song.ProviderSongId, CreationCollisionOption.ReplaceExisting);
                                await stream.SaveToFileAsync(file);

                                if (cache == null)
                                {
                                    cache = new CachedSong() { FileName = file.Name, SongId = nextTask.Song.SongId, TaskAdded = DateTime.Now };
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
            catch (Exception exception)
            {
                if (exception is OperationCanceledException)
                {
                    this.logger.Debug("DownloadAsync was canceled.");
                }
                else
                {
                    this.logger.Error(exception, "Exception while tried to DownloadAsync.");
                }
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
                    var localFolder = ApplicationData.Current.LocalFolder;
                    this.cacheFolder = await localFolder.CreateFolderAsync(SongsCacheFolder, CreationCollisionOption.OpenIfExists);
                }
            }
            finally
            {
                this.mutex.Release(1);
            }
        }

        private string GetFullPath(string fileName)
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, SongsCacheFolder, fileName.Substring(0, 1), fileName);
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
