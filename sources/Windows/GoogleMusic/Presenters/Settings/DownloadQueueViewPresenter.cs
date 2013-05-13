// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels.Settings;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    public class DownloadQueueViewPresenter : DisposableViewPresenterBase<IView>
    {
        private readonly ISongsCachingService cachingService;

        private ObservableCollection<CachedSongBindingModel> queuedTasks;
        private bool isLoading;

        private INetworkRandomAccessStream currentDownloadStream;
        private CachedSongBindingModel currentCachedSong;

        internal DownloadQueueViewPresenter(
            ISongsCachingService cachingService)
        {
            this.cachingService = cachingService;

            this.CancelTaskCommand = new DelegateCommand(CancelTask);
        }

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                this.SetValue(ref this.isLoading, value);
            }
        }

        public ObservableCollection<CachedSongBindingModel> QueuedTasks
        {
            get
            {
                return this.queuedTasks;
            }

            private set
            {
                this.SetValue(ref this.queuedTasks, value);
            }
        }

        public DelegateCommand CancelTaskCommand { get; private set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.RegisterForDispose(this.EventAggregator.GetEvent<SongCachingChangeEvent>().Subscribe(async e => await this.Dispatcher.RunAsync(() => this.OnCacheUpdated(e))));

            this.LoadQueuedTasks();
        }

        protected override void OnDisposing()
        {
            base.OnDisposing();

            var networkRandomAccessStream = this.currentDownloadStream;
            if (networkRandomAccessStream != null)
            {
                networkRandomAccessStream.DownloadProgressChanged -= this.CurrentDownloadStreamOnDownloadProgressChanged;
            }
        }

        private async void LoadQueuedTasks()
        {
            await this.Dispatcher.RunAsync(() => { this.IsLoading = true; });

            IList<CachedSong> cachedSongs = await this.cachingService.GetAllActiveTasksAsync();

            await this.Dispatcher.RunAsync(
                () =>
                {
                    this.QueuedTasks = new ObservableCollection<CachedSongBindingModel>(cachedSongs.Select(x => new CachedSongBindingModel(x)));
                    this.IsLoading = false;
                });

            var currentTask = await this.cachingService.GetCurrentTaskAsync();
            if (currentTask != null)
            {
                await this.Dispatcher.RunAsync(() => this.SubscribeToSong(currentTask.Item2, currentTask.Item1));
            }
        }

        private void OnCacheUpdated(SongCachingChangeEvent e)
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
                    this.QueuedTasks.Remove(this.currentCachedSong);
                }
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
                this.currentDownloadStream.DownloadProgressChanged += this.CurrentDownloadStreamOnDownloadProgressChanged;
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

        private async void CancelTask(object task)
        {
            var cachedSong = task as CachedSongBindingModel;
            if (cachedSong != null)
            {
                await this.Dispatcher.RunAsync(() => { this.IsLoading = true; });

                try
                {
                    await this.cachingService.CancelTaskAsync(cachedSong.CachedSong);
                }
                catch (Exception e)
                {
                    this.Logger.Error(e, "Cannot cancel cached song task.");
                }

                await this.Dispatcher.RunAsync(() =>
                {
                    this.IsLoading = false;
                    this.QueuedTasks.Remove(cachedSong);
                });
            }
        }

        private CachedSongBindingModel FindCachedSongBindingModel(Song song)
        {
            if (song == null)
            {
                return null;
            }

            return this.QueuedTasks.FirstOrDefault(queuedTask => queuedTask.CachedSong.SongId == song.SongId);
        }
    }
}