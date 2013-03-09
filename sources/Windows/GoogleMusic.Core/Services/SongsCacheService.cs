// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Storage;
    using Windows.Storage.Search;

    public class SongsCacheService : ISongsCacheService
    {
        private const string SongsCache = "songs.v1.cache";
        private const string SongsUpdatesCache = "songs.v1.updates.cache";

        private const string LastUpdateKey = "SongsCacheService_CacheFreshnessDate";

        private readonly ISettingsService settingsService;

        private readonly ILogger logger;

        private readonly object locker = new object();

        public SongsCacheService(
            ILogManager logManager,
            ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            this.logger = logManager.CreateLogger("SongsCacheService");
        }

        public async Task SaveToFileAsync(DateTime lastUpdate, IEnumerable<SongMetadata> songs)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            try
            {
                Monitor.Enter(this.locker);

                var localFolder = ApplicationData.Current.LocalFolder;
                var file = await localFolder.CreateFileAsync(SongsCache, CreationCollisionOption.ReplaceExisting);
                string jsonData = JsonConvert.SerializeObject(new SongsCache { LastUpdate = lastUpdate, Songs = songs.ToArray() });
                await FileIO.WriteTextAsync(file, jsonData);

                var fileUpdates = await this.GetSongsUpdatesCacheStorageFileAsync();
                if (fileUpdates != null)
                {
                    await fileUpdates.DeleteAsync();
                }

                this.UpdateCacheFreshness(lastUpdate);
            }
            finally
            {
                Monitor.Exit(this.locker);
            }
        }

        public async Task UpdateSongMedatadaAsync(SongMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            try
            {
                Monitor.Enter(this.locker);

                var localFolder = ApplicationData.Current.LocalFolder;
                var file = await localFolder.CreateFileAsync(SongsUpdatesCache, CreationCollisionOption.OpenIfExists);
                await FileIO.AppendLinesAsync(file, new[] { JsonConvert.SerializeObject(metadata) });
            }
            finally 
            {
                Monitor.Exit(this.locker);
            }
        }

        public async Task<SongsCache> ReadFromFileAsync()
        {
            this.logger.Debug("Trying to find songs cache. File '{0}'.", SongsCache);

            try
            {
                Monitor.Enter(this.locker);

                var file = await this.GetSongsCacheStorageFileAsync();
                if (file != null)
                {
                    string jsonData = await FileIO.ReadTextAsync(file);

                    if (this.logger.IsDebugEnabled)
                    {
                        this.logger.Debug(
                            "Found cache file. Json data length: {0}. File body: {1}.",
                            jsonData.Length,
                            jsonData.Substring(0, Math.Min(100, jsonData.Length)));
                    }

                    var songsCache = JsonConvert.DeserializeObject<SongsCache>(jsonData);

                    var fileUpdates = await this.GetSongsUpdatesCacheStorageFileAsync();
                    if (fileUpdates != null)
                    {
                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug("Found file with updates.");
                        }

                        IList<string> updates = await FileIO.ReadLinesAsync(fileUpdates);
                        if (updates.Count > 0)
                        {
                            if (this.logger.IsDebugEnabled)
                            {
                                this.logger.Debug("File contains {0} lines.", updates.Count);
                            }

                            Dictionary<string, SongMetadata> dictionary = null;
                            if (songsCache.Songs != null)
                            {
                                dictionary = songsCache.Songs.ToDictionary(x => x.Id, x => x);
                            }
                            else
                            {
                                dictionary = new Dictionary<string, SongMetadata>();
                            }

                            foreach (var update in updates)
                            {
                                var updatedMetadata = JsonConvert.DeserializeObject<SongMetadata>(update);

                                // If dictionary does not contains this Id - this can be possible if song has been deleted
                                if (dictionary.ContainsKey(updatedMetadata.Id))
                                {
                                    dictionary[updatedMetadata.Id] = updatedMetadata;
                                }
                            }

                            songsCache.Songs = dictionary.Values.ToArray();
                        }
                    }

                    songsCache.LastUpdate = this.settingsService.GetValue(LastUpdateKey, songsCache.LastUpdate);
                    return songsCache;
                }
                else
                {
                    this.logger.Debug("Cache file does not exists.");
                }
            }
            catch (Exception e)
            {
                this.logger.Error("Could not read data from file. See exception below.");
                this.logger.LogErrorException(e);
            }
            finally
            {
                Monitor.Exit(this.locker);
            }

            return null;
        }

        public async Task ClearCacheAsync()
        {
            var file = await this.GetSongsCacheStorageFileAsync();
            if (file != null)
            {
                await file.DeleteAsync();
            }

            var fileUpdates = await this.GetSongsUpdatesCacheStorageFileAsync();
            if (fileUpdates != null)
            {
                await fileUpdates.DeleteAsync();
            }

            this.settingsService.RemoveValue(LastUpdateKey);
        }

        public void UpdateCacheFreshness(DateTime lastUpdate)
        {
            this.settingsService.SetValue(LastUpdateKey, lastUpdate);
        }

        private Task<StorageFile> GetSongsCacheStorageFileAsync()
        {
            return this.GetStorageFileAsync(SongsCache);
        }

        private Task<StorageFile> GetSongsUpdatesCacheStorageFileAsync()
        {
            return this.GetStorageFileAsync(SongsUpdatesCache);
        }

        private async Task<StorageFile> GetStorageFileAsync(string fileName)
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var files = await localFolder.GetFilesAsync(CommonFileQuery.DefaultQuery);
            return files.FirstOrDefault(f => string.Equals(f.Name, fileName, StringComparison.OrdinalIgnoreCase));
        }
    }
}