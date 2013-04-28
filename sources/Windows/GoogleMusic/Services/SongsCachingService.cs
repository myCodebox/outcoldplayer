// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    using Windows.Networking.BackgroundTransfer;
    using Windows.Storage;
    using Windows.Storage.Streams;

    internal interface ISongsCachingService
    {
        Task<IRandomAccessStreamWithContentType> GetStreamAsync(Song song);

        Task QueueForDownloadAsync(IEnumerable<Song> song, bool isPriorityZero);

        Task<StorageFolder> GetCacheFolderAsync();

        Task ClearCacheAsync();
    }

    internal class SongsCachingService : ISongsCachingService
    {
        private const string SongsCacheFolder = "SongsCache";

        private readonly ISongsWebService songsWebService;
        private readonly ICachedSongsRepository songsCacheRepository;
        private readonly ISongsRepository songsRepository;
        private readonly IMediaStreamDownloadService mediaStreamDownloadService;

        private readonly ILogger logger;
        private readonly BackgroundDownloader backgroundDownloader = new BackgroundDownloader();

        private StorageFolder cacheFolder;

        public SongsCachingService(
            ILogManager logManager,
            ISongsWebService songsWebService,
            ICachedSongsRepository songsCacheRepository,
            ISongsRepository songsRepository,
            IMediaStreamDownloadService mediaStreamDownloadService)
        {
            this.logger = logManager.CreateLogger("SongsCachingService");
            this.songsWebService = songsWebService;
            this.songsCacheRepository = songsCacheRepository;
            this.songsRepository = songsRepository;
            this.mediaStreamDownloadService = mediaStreamDownloadService;
        }

        public async Task<IRandomAccessStreamWithContentType> GetStreamAsync(Song song)
        {
            var cache = await this.songsCacheRepository.FindAsync(song);
            if (cache != null && !string.IsNullOrEmpty(cache.FileName))
            {
                // TODO: Catch exception if file does not exist
                var storageFile = await StorageFile.GetFileFromPathAsync(this.GetFullPath(cache.FileName));
                return await storageFile.OpenReadAsync();
            }

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
                    stream.DownloadProgressChanged += async (sender, d) =>
                        {
                            if (Math.Abs(1 - d) <= 0.0001)
                            {
                                await this.OnCurrentSongDownloadCompletedAsync(stream, song);
                            }
                        };
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

        public async Task QueueForDownloadAsync(IEnumerable<Song> songs, bool isPriorityZero)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            foreach (var song in songs)
            {
                await this.songsCacheRepository.AddAsync(new CachedSong { SongId = song.SongId, TaskAdded = DateTime.Now });
            }

            if (isPriorityZero)
            {
                this.DownloadAsync();
            }
        }

        public async Task<StorageFolder> GetCacheFolderAsync()
        {
            await this.InitializeCacheFolderAsync();
            return this.cacheFolder;
        }

        public async Task ClearCacheAsync()
        {
            await this.songsCacheRepository.ClearCacheAsync();
            foreach (var storageItem in await this.cacheFolder.GetItemsAsync())
            {
                await storageItem.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        private async Task OnCurrentSongDownloadCompletedAsync(INetworkRandomAccessStream randomAccessStream, Song song)
        {
            await this.InitializeCacheFolderAsync();

            var cache = await this.songsCacheRepository.FindAsync(song);
            if (cache == null || string.IsNullOrEmpty(cache.FileName))
            {
                var songFolder = await this.cacheFolder.CreateFolderAsync(song.ProviderSongId.Substring(0, 1), CreationCollisionOption.OpenIfExists);
                var file = await songFolder.CreateFileAsync(song.ProviderSongId, CreationCollisionOption.ReplaceExisting);
                await randomAccessStream.SaveToFileAsync(file);

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
        }

        private async void DownloadAsync()
        {
            await this.InitializeCacheFolderAsync();
            await this.FinishCachingAsync();

            CachedSong nextTask;
            while ((nextTask = await this.songsCacheRepository.GetNextAsync()) != null)
            {
                var cacheGroupFolder = await this.cacheFolder.CreateFolderAsync(nextTask.Song.ProviderSongId.Substring(0, 1), CreationCollisionOption.OpenIfExists);

                var songUrl = await this.songsWebService.GetSongUrlAsync(nextTask.Song.ProviderSongId);
                if (songUrl != null)
                {
                    var songCacheFile = await cacheGroupFolder.CreateFileAsync(nextTask.Song.ProviderSongId, CreationCollisionOption.ReplaceExisting);
                    var downloadOperation = this.backgroundDownloader.CreateDownload(new Uri(songUrl.Url), songCacheFile);
                    await downloadOperation.StartAsync().AsTask();

                    nextTask.FileName = Path.GetFileName(songCacheFile.Path);
                    await this.songsCacheRepository.UpdateAsync(nextTask);
                }
                else
                {
                    // TODO: Show error that could not download.
                    break;
                }
            }
        }

        private async Task FinishCachingAsync()
        {
            IReadOnlyList<DownloadOperation> currentDownloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            if (currentDownloads != null)
            {
                await Task.WhenAll(currentDownloads.Select(x => x.AttachAsync().AsTask()));

                foreach (var downloadOperation in currentDownloads)
                {
                    string fileName = Path.GetFileName(downloadOperation.ResultFile.Path);
                    var song = await this.songsRepository.FindAsync(fileName);
                    if (song != null)
                    {
                        var cache = await this.songsCacheRepository.FindAsync(song);
                        if (cache != null)
                        {
                            cache.FileName = fileName;
                            await this.songsCacheRepository.UpdateAsync(cache);
                        }
                        else
                        {
                            await downloadOperation.ResultFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        }
                    }
                }
            }
        }

        private async Task InitializeCacheFolderAsync()
        {
            if (this.cacheFolder == null)
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                this.cacheFolder = await localFolder.CreateFolderAsync(SongsCacheFolder, CreationCollisionOption.OpenIfExists);
            }
        }

        private string GetFullPath(string fileName)
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, SongsCacheFolder, fileName.Substring(0, 1), fileName);
        }
    }
}
