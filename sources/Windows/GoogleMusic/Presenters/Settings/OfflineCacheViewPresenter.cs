// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels.Settings;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    using Windows.Storage;

    internal class OfflineCacheViewPresenter : DisposableViewPresenterBase<IView>
    {
        private readonly IAlbumArtCacheService albumArtCacheService;
        private readonly ISongsCachingService songsCachingService;
        private readonly ISearchService searchService;
        private readonly ISongsCachingService cachingService;

        private INetworkRandomAccessStream currentDownloadStream;
        private CachedSongBindingModel currentCachedSong;

        public OfflineCacheViewPresenter(
            IAlbumArtCacheService albumArtCacheService,
            ISongsCachingService songsCachingService, 
            OfflineCacheViewBindingModel bindingModel,
            ISearchService searchService,
            ISongsCachingService cachingService)
        {
            this.albumArtCacheService = albumArtCacheService;
            this.songsCachingService = songsCachingService;
            this.searchService = searchService;
            this.cachingService = cachingService;
            this.BindingModel = bindingModel;

            this.BindingModel.IsLoading = true;

            this.searchService.SetShowOnKeyboardInput(false);

            this.ClearAlbumArtsCacheCommand = new DelegateCommand(this.ClearAlbumArtsCache, () => !this.BindingModel.IsLoading);
            this.ClearSongsCacheCommand = new DelegateCommand(this.ClearSongsCache, () => !this.BindingModel.IsLoading);
            this.CancelTaskCommand = new DelegateCommand(this.CancelTask, (e) => !this.BindingModel.IsLoading);
        }

        public OfflineCacheViewBindingModel BindingModel { get; private set; }

        public DelegateCommand ClearAlbumArtsCacheCommand { get; private set; }

        public DelegateCommand ClearSongsCacheCommand { get; private set; }

        public DelegateCommand CancelTaskCommand { get; private set; }

        protected async override void OnInitialized()
        {
            base.OnInitialized();

            await this.UpdateLoadingState(isLoading: true);
            await this.LoadFolderSizesAsync();
            await this.LoadQueuedTasks();
            await this.UpdateLoadingState(isLoading: false);

            this.RegisterForDispose(this.EventAggregator.GetEvent<SongCachingChangeEvent>().Subscribe(async e => await this.Dispatcher.RunAsync(() => this.OnCacheUpdated(e))));
        }

        protected override void OnDisposing()
        {
            base.OnDisposing();

            this.searchService.SetShowOnKeyboardInput(true);

            var networkRandomAccessStream = this.currentDownloadStream;
            if (networkRandomAccessStream != null)
            {
                networkRandomAccessStream.DownloadProgressChanged -= this.CurrentDownloadStreamOnDownloadProgressChanged;
            }
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

        private async Task LoadQueuedTasks()
        {
            IList<CachedSong> cachedSongs = await this.cachingService.GetAllActiveTasksAsync();

            await this.Dispatcher.RunAsync(
                () =>
                {
                    this.BindingModel.QueuedTasks = new ObservableCollection<CachedSongBindingModel>(cachedSongs.Select(x => new CachedSongBindingModel(x)));
                });

            var currentTask = await this.cachingService.GetCurrentTaskAsync();
            if (currentTask != null)
            {
                await this.Dispatcher.RunAsync(() => this.SubscribeToSong(currentTask.Item2, currentTask.Item1));
            }
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

        private async void CancelTask(object task)
        {
            var cachedSong = task as CachedSongBindingModel;
            if (cachedSong != null)
            {
                await this.UpdateLoadingState(isLoading: true);

                try
                {
                    await this.cachingService.CancelTaskAsync(cachedSong.CachedSong);
                }
                catch (Exception e)
                {
                    this.Logger.Error(e, "Cannot cancel cached song task.");
                }

                await this.UpdateLoadingState(isLoading: false);
                await this.Dispatcher.RunAsync(() => this.BindingModel.QueuedTasks.Remove(cachedSong));
            }
        }

        private async void OnCacheUpdated(SongCachingChangeEvent e)
        {
            if (e.EventType == SongCachingChangeEventType.StartDownloading)
            {
                this.ClearCurrentCachedSongSubscription();

                this.SubscribeToSong(e.Song, e.Stream);
            }
            else if (e.EventType == SongCachingChangeEventType.FinishDownloading)
            {
                this.ClearCurrentCachedSongSubscription();

                if (this.currentCachedSong == null)
                {
                    this.currentCachedSong = this.FindCachedSongBindingModel(e.Song);
                }

                if (this.currentCachedSong != null)
                {
                    this.BindingModel.QueuedTasks.Remove(this.currentCachedSong);
                }

                await this.Dispatcher.RunAsync(
                    () =>
                    {
                        this.BindingModel.SongsCacheSize += (long)e.Stream.Size;
                    });
            }
        }

        private void SubscribeToSong(Song song, INetworkRandomAccessStream networkRandomAccessStream)
        {
            var queuedTask = this.FindCachedSongBindingModel(song);
            if (queuedTask != null)
            {
                queuedTask.IsDownloading = true;
                this.currentCachedSong = queuedTask;
                this.currentDownloadStream = networkRandomAccessStream;
                if (this.currentDownloadStream != null)
                {
                    this.currentDownloadStream.DownloadProgressChanged += this.CurrentDownloadStreamOnDownloadProgressChanged;
                }
            }
        }

        private void ClearCurrentCachedSongSubscription()
        {
            if (this.currentCachedSong != null)
            {
                this.currentCachedSong.IsDownloading = false;
                this.currentCachedSong = null;
                if (this.currentDownloadStream != null)
                {
                    this.currentDownloadStream.DownloadProgressChanged -= this.CurrentDownloadStreamOnDownloadProgressChanged;
                }
            }
        }

        private async void CurrentDownloadStreamOnDownloadProgressChanged(object sender, double d)
        {
            if (this.currentCachedSong != null)
            {
                await this.Dispatcher.RunAsync(() => { this.currentCachedSong.DownloadProgress = d; });
            }
        }

        private CachedSongBindingModel FindCachedSongBindingModel(Song song)
        {
            if (song == null)
            {
                return null;
            }

            return this.BindingModel.QueuedTasks.FirstOrDefault(queuedTask => queuedTask.CachedSong.SongId == song.SongId);
        }
    }
}
