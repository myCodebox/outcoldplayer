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
            this.ClearAlbumCacheCommand = new DelegateCommand(this.ClearAlbumCache, () => !this.BindingModel.IsLoading);
        }

        public OfflineCacheViewBindingModel BindingModel { get; private set; }

        public DelegateCommand ClearAlbumCacheCommand { get; private set; }

        protected async override void OnInitialized()
        {
            base.OnInitialized();

            await this.Dispatcher.RunAsync(() =>
                {
                    this.BindingModel.IsLoading = true;
                    this.ClearAlbumCacheCommand.RaiseCanExecuteChanged();
                });
            await this.LoadFolderSizesAsync();
            await this.Dispatcher.RunAsync(() =>
                    {
                        this.BindingModel.IsLoading = false;
                        this.ClearAlbumCacheCommand.RaiseCanExecuteChanged();
                    });
        }

        private async void ClearAlbumCache()
        {
            await this.Dispatcher.RunAsync(() =>
            {
                this.BindingModel.IsLoading = true;
                this.ClearAlbumCacheCommand.RaiseCanExecuteChanged();
            });

            await this.albumArtCacheService.ClearCacheAsync();
            await this.LoadAlbumArtsCacheFolderSizeAsync();

            await this.Dispatcher.RunAsync(() =>
            {
                this.BindingModel.IsLoading = false;
                this.ClearAlbumCacheCommand.RaiseCanExecuteChanged();
            });
        }

        private async Task LoadFolderSizesAsync()
        {
            await this.LoadAlbumArtsCacheFolderSizeAsync();
            await this.LoadSongsCacheFolderSizeAsync();
        }

        private async Task LoadSongsCacheFolderSizeAsync()
        {
            this.BindingModel.SongsCacheSize = 0;

            StorageFolder songsCacheFolder = await this.songsCachingService.GetCacheFolderAsync();
            foreach (var subfolder in await songsCacheFolder.GetFoldersAsync())
            {
                var basicProperties =
                    await
                    Task.WhenAll((await subfolder.GetFilesAsync()).Select(f => f.GetBasicPropertiesAsync().AsTask()).ToArray());
                long folderSize = basicProperties.Sum(p => (long)p.Size);
                await this.Dispatcher.RunAsync(() => this.BindingModel.SongsCacheSize += folderSize);
            }
        }

        private async Task LoadAlbumArtsCacheFolderSizeAsync()
        {
            this.BindingModel.AlbumArtCacheSize = 0;

            StorageFolder albumArtCacheFolder = await this.albumArtCacheService.GetCacheFolderAsync();
            foreach (var subfolder in await albumArtCacheFolder.GetFoldersAsync())
            {
                var basicProperties =
                    await
                    Task.WhenAll((await subfolder.GetFilesAsync()).Select(f => f.GetBasicPropertiesAsync().AsTask()).ToArray());
                long folderSize = basicProperties.Sum(p => (long)p.Size);
                await this.Dispatcher.RunAsync(() => this.BindingModel.AlbumArtCacheSize += folderSize);
            }
        }
    }
}
