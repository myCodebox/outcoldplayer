// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Storage;
    using Windows.Storage.Search;

    public class SongsCacheService : ISongsCacheService
    {
        private const string SongsCache = "songs.v1.cache";

        private const string LastUpdateKey = "SongsCacheService_CacheFreshnessDate";

        private readonly ISettingsService settingsService;

        private readonly ILogger logger;

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

            var localFolder = ApplicationData.Current.LocalFolder;
            var file = await localFolder.CreateFileAsync(SongsCache, CreationCollisionOption.ReplaceExisting);
            string jsonData = JsonConvert.SerializeObject(new SongsCache { LastUpdate = lastUpdate, Songs = songs.ToArray() });
            await FileIO.WriteTextAsync(file, jsonData);

            this.UpdateCacheFreshness(lastUpdate);
        }

        public async Task<SongsCache> ReadFromFileAsync()
        {
            this.logger.Debug("Trying to find songs cache. File '{0}'.", SongsCache);

            try
            {
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

            return null;
        }

        public async Task ClearCacheAsync()
        {
            var file = await this.GetSongsCacheStorageFileAsync();
            if (file != null)
            {
                await file.DeleteAsync();
                this.settingsService.RemoveValue(LastUpdateKey);
            }
        }

        public void UpdateCacheFreshness(DateTime lastUpdate)
        {
            this.settingsService.SetValue(LastUpdateKey, lastUpdate);
        }

        private async Task<StorageFile> GetSongsCacheStorageFileAsync()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var files = await localFolder.GetFilesAsync(CommonFileQuery.DefaultQuery);
            return files.FirstOrDefault(f => string.Equals(f.Name, SongsCache, StringComparison.OrdinalIgnoreCase));
        }
    }
}