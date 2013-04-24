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
    }

    internal class SongsCachingService : ISongsCachingService
    {
        private const string SongsCacheFolder = "SongsCache";

        private readonly ISongsWebService songsWebService;
        private readonly ICachedSongsRepository cacheTasksRepository;
        private readonly IMediaStreamDownloadService mediaStreamDownloadService;

        private readonly ILogger logger;

        private readonly BackgroundDownloader backgroundDownloader = new BackgroundDownloader();

        public SongsCachingService(
            ILogManager logManager,
            ISongsWebService songsWebService,
            ICachedSongsRepository cacheTasksRepository,
            IMediaStreamDownloadService mediaStreamDownloadService)
        {
            this.logger = logManager.CreateLogger("SongsCachingService");
            this.songsWebService = songsWebService;
            this.cacheTasksRepository = cacheTasksRepository;
            this.mediaStreamDownloadService = mediaStreamDownloadService;

            // this.DownloadAsync();
        }

        public async Task<IRandomAccessStreamWithContentType> GetStreamAsync(Song song)
        {
            var cache = await this.cacheTasksRepository.FindAsync(song);
            if (cache != null && !string.IsNullOrEmpty(cache.Path))
            {
                // TODO: Catch exception if file does not exist
                var storageFile = await StorageFile.GetFileFromPathAsync(cache.Path);
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
                    return await this.mediaStreamDownloadService.GetStreamAsync(songUrl.Url);
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
                await this.cacheTasksRepository.AddAsync(new CachedSong { SongId = song.SongId, TaskAdded = DateTime.Now });
            }

            if (isPriorityZero)
            {
                this.DownloadAsync();
            }
        }

        private async void DownloadAsync()
        {
            IReadOnlyList<DownloadOperation> currentDownloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            if (currentDownloads != null)
            {
                await Task.WhenAll(currentDownloads.Select(x => x.StartAsync().AsTask()));
            }

            CachedSong nextTask;
            while ((nextTask = await this.cacheTasksRepository.GetNextAsync()) != null)
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var cacheFolder = await localFolder.CreateFolderAsync(SongsCacheFolder, CreationCollisionOption.OpenIfExists);
                var cacheGroupFolder = await cacheFolder.CreateFolderAsync(nextTask.Song.ProviderSongId.Substring(0, 1), CreationCollisionOption.OpenIfExists);

                var songUrl = await this.songsWebService.GetSongUrlAsync(nextTask.Song.ProviderSongId);
                if (songUrl != null)
                {
                    var songCacheFile = await cacheGroupFolder.CreateFileAsync(nextTask.Song.ProviderSongId, CreationCollisionOption.ReplaceExisting);
                    var downloadOperation = this.backgroundDownloader.CreateDownload(new Uri(songUrl.Url), songCacheFile);
                    await downloadOperation.StartAsync().AsTask();

                    nextTask.Path = songCacheFile.Path;
                    await this.cacheTasksRepository.UpdateAsync(nextTask);
                }
                else
                {
                    // TODO: Show error that could not download.
                    break;
                }
            }
        }
    }
}
