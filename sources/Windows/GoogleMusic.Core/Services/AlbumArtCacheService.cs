// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;

    using Windows.Storage;

    public class AlbumArtCacheService : IAlbumArtCacheService
    {
        private const string AlbumArtCacheFolder = "AlbumArtCache";

        private readonly ILogger logger;
        private readonly IApplicationStateService stateService;
        private readonly ICachedAlbumArtsRepository cachedAlbumArtsRepository;

        private readonly SemaphoreSlim limitSemaphore = new SemaphoreSlim(4);
        private readonly SemaphoreSlim dataSemaphore = new SemaphoreSlim(1);
        private readonly Dictionary<CachedKey, Task<CachedAlbumArt>> downloadTasks = new Dictionary<CachedKey, Task<CachedAlbumArt>>();
        private readonly HttpClient httpClient = new HttpClient();
        private StorageFolder cacheFolder;

        public AlbumArtCacheService(
            ILogManager logManager, 
            IApplicationStateService stateService,
            ICachedAlbumArtsRepository cachedAlbumArtsRepository)
        {
            this.logger = logManager.CreateLogger("AlbumArtCacheService");
            this.stateService = stateService;
            this.cachedAlbumArtsRepository = cachedAlbumArtsRepository;
        }

        public async Task<string> GetCachedImageAsync(Uri url, uint size)
        {
            await this.InitializeCacheFolderAsync();

            CachedAlbumArt cache = await this.cachedAlbumArtsRepository.FindAsync(url, size);

            if (cache == null && this.stateService.IsOffline())
            {
                return null;
            }

            if (cache == null)
            {
                await this.limitSemaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

                try
                {
                    Task<CachedAlbumArt> task;
                    CachedKey cachedKey = new CachedKey(url, size);

                    try
                    {
                        await this.dataSemaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
                       
                        if (!this.downloadTasks.TryGetValue(cachedKey, out task))
                        {
                            this.downloadTasks.Add(cachedKey, task = this.GetCachedAlbumArtAsync(cachedKey));
                        }
                    }
                    finally 
                    {
                        this.dataSemaphore.Release(1);
                    }

                    cache = await task;
                }
                finally
                {
                    
                    this.limitSemaphore.Release(1);
                }
            }

            return Path.Combine(AlbumArtCacheFolder, cache.FileName.Substring(0, 1), cache.FileName);
        }

        public async Task DeleteBrokenLinkAsync(Uri url, uint size)
        {
            try
            {
                await this.dataSemaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
                this.downloadTasks.Remove(new CachedKey(url, size));
            }
            finally
            {
                this.dataSemaphore.Release(1);
            }

            await this.cachedAlbumArtsRepository.DeleteBrokenLinkAsync(url, size);
        }

        public async Task<StorageFolder> GetCacheFolderAsync()
        {
            await this.InitializeCacheFolderAsync();
            return this.cacheFolder;
        }

        public async Task ClearCacheAsync()
        {
            try
            {
                await this.dataSemaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
                this.downloadTasks.Clear();
            }
            finally
            {
                this.dataSemaphore.Release(1);
            }

            await this.cachedAlbumArtsRepository.ClearCacheAsync();
            var folder = await this.GetCacheFolderAsync();
            foreach (var storageItem in await folder.GetItemsAsync())
            {
                await storageItem.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        private async Task<CachedAlbumArt> GetCachedAlbumArtAsync(CachedKey cachedKey)
        {
            CachedAlbumArt downloadedCache = await this.cachedAlbumArtsRepository.FindAsync(cachedKey.AlbumArtUrl, cachedKey.Size);
            if (downloadedCache == null)
            {
                string fileName = Guid.NewGuid().ToString();
                string subFolderName = fileName.Substring(0, 1);

                var folder = await this.cacheFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.OpenIfExists);
                var file = await folder.CreateFileAsync(fileName);

                using (var imageStream = await this.httpClient.GetStreamAsync(cachedKey.AlbumArtUrl.ChangeSize(cachedKey.Size)))
                {
                    using (var targetStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        using (Stream fileStream = targetStream.AsStreamForWrite())
                        {
                            await imageStream.CopyToAsync(fileStream);
                            await fileStream.FlushAsync();
                        }
                    }
                }

                downloadedCache = new CachedAlbumArt() { AlbumArtUrl = cachedKey.AlbumArtUrl, Size = cachedKey.Size, FileName = fileName };

                try
                {
                    await this.cachedAlbumArtsRepository.AddAsync(downloadedCache);
                }
                catch (Exception e)
                {
                    this.logger.Debug(e, "Could not insert the downloaded cache");
                }
            }

            return downloadedCache;
        }

        private async Task InitializeCacheFolderAsync()
        {
            await this.dataSemaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                if (this.cacheFolder == null)
                {
                    var localFolder = ApplicationData.Current.LocalFolder;
                    this.cacheFolder =
                        await localFolder.CreateFolderAsync(AlbumArtCacheFolder, CreationCollisionOption.OpenIfExists);
                    this.DeleteRemovedItems();
                }
            }
            finally
            {
                this.dataSemaphore.Release(1);
            }
        }

        private async void DeleteRemovedItems()
        {
            try
            {
                var removedItems = await this.cachedAlbumArtsRepository.GetRemovedCachedItemsAsync();
                foreach (var cache in removedItems)
                {
                    try
                    {
                        try
                        {
                            await this.dataSemaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
                            this.downloadTasks.Remove(new CachedKey(cache.AlbumArtUrl, cache.Size));
                        }
                        finally
                        {
                            this.dataSemaphore.Release(1);
                        }

                        var file = await ApplicationData.Current.LocalFolder.GetFileAsync(Path.Combine(AlbumArtCacheFolder, cache.FileName.Substring(0, 1), cache.FileName));
                        if (file != null)
                        {
                            try
                            {
                                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                            }
                            catch (FileNotFoundException e)
                            {
                                this.logger.Debug(e, "DeleteRemovedItems: Could not delete cached image {0}, because file was not found.", file.Path);
                            }
                            catch (Exception e)
                            {
                                this.logger.Warning(e, "DeleteRemovedItems: Could not delete cached image {0}.", file.Path);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        this.logger.Debug(exception, "Could not delete file of removed album art item.");
                    }
                }

                await this.cachedAlbumArtsRepository.DeleteCachedItemsAsync(removedItems);
            }
            catch (Exception e)
            {
                this.logger.Error(e, "DeleteCachedItemsAsync failed.");
            }
        }

        private class CachedKey : IEquatable<CachedKey>
        {
            public CachedKey(Uri albumArtUrl, uint size)
            {
                this.AlbumArtUrl = albumArtUrl;
                this.Size = size;
            }

            public Uri AlbumArtUrl { get; private set; }

            public uint Size { get; private set; }

            public bool Equals(CachedKey other)
            {
                return object.Equals(this.AlbumArtUrl, other.AlbumArtUrl) && this.Size == other.Size;
            }

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is CachedKey && this.Equals((CachedKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((this.AlbumArtUrl != null ? this.AlbumArtUrl.GetHashCode() : 0) * 397) ^ (int)this.Size;
                }
            }
        }
    }
}
