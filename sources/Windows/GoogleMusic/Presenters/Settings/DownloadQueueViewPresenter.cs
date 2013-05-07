// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using System;
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    public class DownloadQueueViewPresenter : DisposableViewPresenterBase<IView>
    {
        private readonly ISongsCachingService cachingService;

        private IList<CachedSong> queuedTasks;
        private bool isLoading;

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

        public IList<CachedSong> QueuedTasks
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
            this.LoadQueuedTasks();
        }

        private async void LoadQueuedTasks()
        {
            await this.Dispatcher.RunAsync(() => { this.IsLoading = true; });

            IList<CachedSong> cachedSongs = await this.cachingService.GetAllActiveTasksAsync();

            await this.Dispatcher.RunAsync(
                () =>
                {
                    this.QueuedTasks = cachedSongs;
                    this.IsLoading = false;
                });
        }

        private async void CancelTask(object task)
        {
            var cachedSong = task as CachedSong;
            if (cachedSong != null)
            {
                await this.Dispatcher.RunAsync(() => { this.IsLoading = true; });

                try
                {
                    await this.cachingService.CancelTaskAsync(cachedSong);
                }
                catch (Exception e)
                {
                    this.Logger.Error(e, "Cannot cancel cached song task.");
                }

                this.LoadQueuedTasks();
            }
        }
    }
}