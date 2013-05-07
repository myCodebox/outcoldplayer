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
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Shell;
    using OutcoldSolutions.Views;

    using Windows.Storage;

    internal class OfflineCacheViewPresenter : DisposableViewPresenterBase<IView>
    {
        private readonly IAlbumArtCacheService albumArtCacheService;
        private readonly ISongsCachingService songsCachingService;
        private readonly ISearchService searchService;

        private readonly IApplicationSettingViewsService applicationSettingViewsService;

        public OfflineCacheViewPresenter(
            IAlbumArtCacheService albumArtCacheService,
            ISongsCachingService songsCachingService, 
            OfflineCacheViewBindingModel bindingModel,
            ISearchService searchService,
            IApplicationSettingViewsService applicationSettingViewsService)
        {
            this.albumArtCacheService = albumArtCacheService;
            this.songsCachingService = songsCachingService;
            this.searchService = searchService;
            this.applicationSettingViewsService = applicationSettingViewsService;
            this.BindingModel = bindingModel;

            this.BindingModel.IsLoading = true;

            this.searchService.SetShowOnKeyboardInput(false);

            this.ClearAlbumArtsCacheCommand = new DelegateCommand(this.ClearAlbumArtsCache, () => !this.BindingModel.IsLoading);
            this.ClearSongsCacheCommand = new DelegateCommand(this.ClearSongsCache, () => !this.BindingModel.IsLoading);
            this.ShowDownloadQueueCommand = new DelegateCommand(this.ShowDownloadQueue);
        }

        public OfflineCacheViewBindingModel BindingModel { get; private set; }

        public DelegateCommand ClearAlbumArtsCacheCommand { get; private set; }

        public DelegateCommand ClearSongsCacheCommand { get; private set; }

        public DelegateCommand ShowDownloadQueueCommand { get; private set; }

        protected async override void OnInitialized()
        {
            base.OnInitialized();

            await this.UpdateLoadingState(isLoading: true);
            await this.LoadFolderSizesAsync();
            await this.UpdateLoadingState(isLoading: false);
        }

        protected override void OnDisposing()
        {
            base.OnDisposing();

            this.searchService.SetShowOnKeyboardInput(true);
        }

        private void ShowDownloadQueue()
        {
            this.applicationSettingViewsService.Close();
            this.applicationSettingViewsService.Show("downloadqueue");
        }

        private async void ClearAlbumArtsCache()
        {
            await this.UpdateLoadingState(isLoading: true);
            await this.albumArtCacheService.ClearCacheAsync();
            await this.LoadAlbumArtsCacheFolderSizeAsync();
            await this.UpdateLoadingState(isLoading: false);
        }

        private async void ClearSongsCache()
        {
            await this.UpdateLoadingState(isLoading: true);
            await this.songsCachingService.ClearCacheAsync();
            await this.LoadSongsCacheFolderSizeAsync();
            await this.UpdateLoadingState(isLoading: false);
        }

        private async Task UpdateLoadingState(bool isLoading)
        {
            await this.Dispatcher.RunAsync(
                () =>
                    {
                        this.BindingModel.IsLoading = isLoading;
                        this.ClearAlbumArtsCacheCommand.RaiseCanExecuteChanged();
                        this.ClearSongsCacheCommand.RaiseCanExecuteChanged();
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
