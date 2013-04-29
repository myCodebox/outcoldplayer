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

    internal interface ISongsCachingService
    {
        Task<IRandomAccessStreamWithContentType> GetStreamAsync(Song song);

        Task PredownloadStreamAsync(Song song);

        Task QueueForDownloadAsync(IEnumerable<Song> song);

        Task<StorageFolder> GetCacheFolderAsync();

        Task ClearCacheAsync();
    }

    internal class SongsCachingService : ISongsCachingService
    {
        private const string SongsCacheFolder = "SongsCache";

        private readonly ISongsWebService songsWebService;
        private readonly ICachedSongsRepository songsCacheRepository;
        private readonly IMediaStreamDownloadService mediaStreamDownloadService;

        private readonly ILogger logger;

        private readonly SemaphoreSlim currentDownloadStreamMutex = new SemaphoreSlim(1);
        private readonly SemaphoreSlim downloadTaskMutex = new SemaphoreSlim(1);

        private StorageFolder cacheFolder;

        private INetworkRandomAccessStream currentDownloadStream;
        private Song currentDownloadSong;

        private Task downloadTask;
        private CancellationTokenSource downloadTaskCancellationToken;

        public SongsCachingService(
            ILogManager logManager,
            ISongsWebService songsWebService,
            ICachedSongsRepository songsCacheRepository,
            IMediaStreamDownloadService mediaStreamDownloadService)
        {
            this.logger = logManager.CreateLogger("SongsCachingService");
            this.songsWebService = songsWebService;
            this.songsCacheRepository = songsCacheRepository;
            this.mediaStreamDownloadService = mediaStreamDownloadService;
        }

        public async Task<IRandomAccessStreamWithContentType> GetStreamAsync(Song song)
        {
            if (song == null)
            {
                throw new ArgumentNullException("song");
            }

            await this.currentDownloadStreamMutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                var cache = await this.songsCacheRepository.FindAsync(song);
                if (cache != null && !string.IsNullOrEmpty(cache.FileName))
                {
                    // TODO: Catch exception if file does not exist
                    var storageFile = await StorageFile.GetFileFromPathAsync(this.GetFullPath(cache.FileName));
                    return await storageFile.OpenReadAsync();
                }

                if (this.currentDownloadSong != null && this.currentDownloadSong.SongId == song.SongId)
                {
                    return this.currentDownloadStream;
                }
            }
            finally
            {
                this.currentDownloadStreamMutex.Release();
            }

            await this.CancelDownloadTaskAsync();

            INetworkRandomAccessStream networkRandomAccessStream = await this.GetNetworkStreamAsync(song);

            await this.SetCurrentStreamAsync(song, networkRandomAccessStream);

            if (networkRandomAccessStream != null)
            {
                this.HandleStreamDownload(networkRandomAccessStream, song);
            }

            return networkRandomAccessStream;
        }

        public async Task PredownloadStreamAsync(Song song)
        {
            if (song == null)
            {
                throw new ArgumentNullException("song");
            }

            await this.currentDownloadStreamMutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                var cache = await this.songsCacheRepository.FindAsync(song);
                if (cache != null && !string.IsNullOrEmpty(cache.FileName))
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
                this.currentDownloadStreamMutex.Release();
            }

            await this.CancelDownloadTaskAsync();

            INetworkRandomAccessStream networkRandomAccessStream = await this.GetNetworkStreamAsync(song);

            await this.SetCurrentStreamAsync(song, networkRandomAccessStream);

            if (networkRandomAccessStream != null)
            {
                this.HandleStreamDownload(networkRandomAccessStream, song);
            }
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
            await this.InitializeCacheFolderAsync();
            await this.songsCacheRepository.ClearCacheAsync();
            foreach (var storageItem in await this.cacheFolder.GetItemsAsync())
            {
                await storageItem.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        private async Task SetCurrentStreamAsync(Song song, INetworkRandomAccessStream networkRandomAccessStream)
        {
            await this.currentDownloadStreamMutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                this.currentDownloadStream = networkRandomAccessStream;
                this.currentDownloadSong = song;
            }
            finally
            {
                this.currentDownloadStreamMutex.Release();
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
                    var stream = await this.mediaStreamDownloadService.GetStreamAsync(songUrl.Url);
                    this.HandleStreamDownload(stream, song);
                    return stream;
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

        private async void HandleStreamDownload(INetworkRandomAccessStream stream, Song song)
        {
            await this.HandleStreamDownloadAsync(stream, song);
            await this.StartDownloadTaskAsync();
        }

        private async Task HandleStreamDownloadAsync(INetworkRandomAccessStream stream, Song song)
        {
            await this.InitializeCacheFolderAsync();

            try
            {
                await stream.DownloadAsync();

                await this.currentDownloadStreamMutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

                try
                {
                    var cache = await this.songsCacheRepository.FindAsync(song);
                    if (cache == null || string.IsNullOrEmpty(cache.FileName))
                    {
                        var songFolder = await this.cacheFolder.CreateFolderAsync(song.ProviderSongId.Substring(0, 1), CreationCollisionOption.OpenIfExists);
                        var file = await songFolder.CreateFileAsync(song.ProviderSongId, CreationCollisionOption.ReplaceExisting);
                        await stream.SaveToFileAsync(file);

                        if (cache == null)
                        {
                            cache = new CachedSong() { FileName = file.Name, SongId = song.SongId, TaskAdded = DateTime.Now };
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
                    this.currentDownloadStreamMutex.Release(1);
                }

                await this.InitializeCacheFolderAsync();
            }
            catch (Exception exception)
            {
                if (exception is TaskCanceledException)
                {
                    this.logger.Debug("HandleStreamDownload was cancelled.");
                }
                else
                {
                    this.logger.Error(exception, "Exception while tried to HandleStreamDownload.");
                }
            }
        }

        private async Task StartDownloadTaskAsync()
        {
            await this.downloadTaskMutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

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
                this.downloadTaskMutex.Release(1);
            }
        }

        private async Task CancelDownloadTaskAsync()
        {
            await this.downloadTaskMutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                if (this.downloadTaskCancellationToken != null)
                {
                    this.downloadTaskCancellationToken.Cancel();
                }

                this.downloadTask = null;
            }
            finally
            {
                this.downloadTaskMutex.Release(1);
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

                    var stream = await this.GetNetworkStreamAsync(nextTask.Song);
                    
                    if (stream == null)
                    {
                        // TODO: Show something to user.
                        break;
                    }
                    else
                    {
                        await this.SetCurrentStreamAsync(nextTask.Song, stream);
                        await this.HandleStreamDownloadAsync(stream, nextTask.Song);
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception is TaskCanceledException)
                {
                    this.logger.Debug("DownloadAsync was cancelled.");
                }
                else
                {
                    this.logger.Error(exception, "Exception while tried to DownloadAsync.");
                }
            }

            await this.downloadTaskMutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                this.downloadTask = null;
            }
            finally
            {
                this.downloadTaskMutex.Release(1);
            }
        }

        private async Task InitializeCacheFolderAsync()
        {
            await this.currentDownloadStreamMutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

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
                this.currentDownloadStreamMutex.Release(1);
            }
        }

        private string GetFullPath(string fileName)
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, SongsCacheFolder, fileName.Substring(0, 1), fileName);
        }
    }
}
