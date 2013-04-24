// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;

    using Windows.Storage;

    public class AlbumArtCacheService : IAlbumArtCacheService
    {
        private const string AlbumArtCacheFolder = "AlbumArtCache";
        private readonly ICachedAlbumArtsRepository cachedAlbumArtsRepository;

        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(2);
        private readonly ConcurrentDictionary<Uri, Task<CachedAlbumArt>> downloadTasks = new ConcurrentDictionary<Uri, Task<CachedAlbumArt>>();
        private readonly HttpClient httpClient = new HttpClient();
        private StorageFolder cacheFolder;

        public AlbumArtCacheService(ICachedAlbumArtsRepository cachedAlbumArtsRepository)
        {
            this.cachedAlbumArtsRepository = cachedAlbumArtsRepository;
        }

        public async Task<string> GetCachedImageAsync(Uri url)
        {
            if (this.cacheFolder == null)
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                this.cacheFolder = await localFolder.CreateFolderAsync(AlbumArtCacheFolder, CreationCollisionOption.OpenIfExists);
            }

            CachedAlbumArt cache = await this.cachedAlbumArtsRepository.FindAsync(url);
            if (cache == null)
            {
                await this.semaphoreSlim.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

                try
                {
                    cache = await this.downloadTasks.GetOrAdd(
                        url,
                        uri => Task.Run(
                            async () =>
                                {
                                    CachedAlbumArt downloadedCache = await this.cachedAlbumArtsRepository.FindAsync(url);
                                    if (downloadedCache == null)
                                    {
                                        string fileName = Guid.NewGuid().ToString() + ".jpg";
                                        string subFolderName = fileName.Substring(0, 1);

                                        var folder = await this.cacheFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.OpenIfExists);
                                        var file = await folder.CreateFileAsync(fileName);

                                        using (var imageStream = await this.httpClient.GetStreamAsync(url))
                                        {
                                            var targetStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                                            using (Stream fileStream = targetStream.AsStreamForWrite())
                                            {
                                                await imageStream.CopyToAsync(fileStream);
                                                await fileStream.FlushAsync();
                                            }
                                        }

                                        downloadedCache = new CachedAlbumArt() { AlbumArtUrl = url, Path = Path.Combine(subFolderName, fileName) };

                                        await this.cachedAlbumArtsRepository.AddAsync(downloadedCache);
                                    }

                                    return downloadedCache;
                                }));

                    Task<CachedAlbumArt> task;
                    this.downloadTasks.TryRemove(url, out task);
                }
                finally
                {
                    this.semaphoreSlim.Release();
                }
            }

            return Path.Combine(AlbumArtCacheFolder, cache.Path);
        }
    }
}
