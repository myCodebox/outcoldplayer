// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels.Settings;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    using Windows.Storage;

    internal class OfflineCacheViewPresenter : ViewPresenterBase<IView>
    {
        private readonly IAlbumArtCacheService albumArtCacheService;
        private readonly ISongsCachingService songsCachingService;

        public OfflineCacheViewPresenter(
            IAlbumArtCacheService albumArtCacheService,
            ISongsCachingService songsCachingService)
        {
            this.albumArtCacheService = albumArtCacheService;
            this.songsCachingService = songsCachingService;
            this.BindingModel = new OfflineCacheViewBindingModel { IsLoading = true };
        }

        public OfflineCacheViewBindingModel BindingModel { get; private set; }

        protected async override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.AlbumArtCacheSize = 0;
            this.BindingModel.SongsCacheSize = 0;

            StorageFolder albumArtCacheFolder = await this.albumArtCacheService.GetCacheFolderAsync();
            foreach (var subfolder in await albumArtCacheFolder.GetFoldersAsync())
            {
                var basicProperties = await Task.WhenAll((await subfolder.GetFilesAsync()).Select(f => f.GetBasicPropertiesAsync().AsTask()).ToArray());
                long folderSize = basicProperties.Sum(p => (long)p.Size);
                await this.Dispatcher.RunAsync(() => this.BindingModel.AlbumArtCacheSize += folderSize);
            }

            StorageFolder songsCacheFolder = await this.songsCachingService.GetCacheFolderAsync();
            foreach (var subfolder in await songsCacheFolder.GetFoldersAsync())
            {
                var basicProperties = await Task.WhenAll((await subfolder.GetFilesAsync()).Select(f => f.GetBasicPropertiesAsync().AsTask()).ToArray());
                long folderSize = basicProperties.Sum(p => (long)p.Size);
                await this.Dispatcher.RunAsync(() => this.BindingModel.SongsCacheSize += folderSize);
            }

            this.BindingModel.IsLoading = false;
        }
    }
}
