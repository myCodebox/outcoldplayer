// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Shell;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml;

    public class LinksRegionViewPresenter : ViewPresenterBase<IView>
    {
        private readonly IApplicationStateService stateService;
        private readonly IApplicationResources resources;
        private readonly IDispatcher dispatcher;
        private readonly IGoogleMusicSynchronizationService googleMusicSynchronizationService;
        private readonly IGoogleMusicSessionService sessionService;

        private readonly DispatcherTimer synchronizationTimer;
        private int synchronizationTime = 0; // we don't want to synchronize playlists each time, so we will do it on each 6 time

        private bool isDownloading = false;
        private bool disableClickToCache = false;

        public LinksRegionViewPresenter(
            IApplicationStateService stateService,
            IApplicationResources resources,
            ISearchService searchService,
            IDispatcher dispatcher,
            IGoogleMusicSynchronizationService googleMusicSynchronizationService,
            IApplicationSettingViewsService applicationSettingViewsService,
            IGoogleMusicSessionService sessionService)
        {
            this.stateService = stateService;
            this.resources = resources;
            this.dispatcher = dispatcher;
            this.googleMusicSynchronizationService = googleMusicSynchronizationService;
            this.sessionService = sessionService;
            this.ShowSearchCommand = new DelegateCommand(searchService.Activate);
            this.NavigateToDownloadQueue = new DelegateCommand(async () =>
            {
                if (!this.disableClickToCache)
                {
                    await this.dispatcher.RunAsync(() => applicationSettingViewsService.Show("offlinecache"));
                }
            });

            this.BindingModel = new LinksRegionBindingModel();

            this.synchronizationTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            this.synchronizationTimer.Stop();
            this.synchronizationTime = 0;

            this.synchronizationTimer.Tick += this.SynchronizationTimerOnTick;

            this.Logger.LogTask(this.Synchronize());

            this.SetOfflineMessageIfRequired();

            this.sessionService.SessionCleared += SessionServiceOnSessionCleared;
        }

        private void SessionServiceOnSessionCleared(object sender, EventArgs eventArgs)
        {
            this.synchronizationTimer.Stop();
            this.sessionService.SessionCleared -= SessionServiceOnSessionCleared;
        }

        public DelegateCommand ShowSearchCommand { get; private set; }

        public DelegateCommand NavigateToDownloadQueue { get; private set; }

        public LinksRegionBindingModel BindingModel { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.EventAggregator.GetEvent<CachingChangeEvent>()
                                .Subscribe(async e => await this.dispatcher.RunAsync(() => this.OnCachingEvent(e.EventType)));

            this.EventAggregator.GetEvent<ApplicationStateChangeEvent>()
                                .Subscribe(async e => await this.dispatcher.RunAsync(this.SetOfflineMessageIfRequired));
        }

        private void SetOfflineMessageIfRequired()
        {
            this.isDownloading = false;

            if (this.stateService.IsOffline())
            {
                this.BindingModel.MessageText = "Offline mode (listen only)";
            }
            else
            {
                this.BindingModel.MessageText = null;
            }
        }

        private async void OnCachingEvent(SongCachingChangeEventType eventType)
        {
            switch (eventType)
            {
                case SongCachingChangeEventType.StartDownloading:
                {
                    this.isDownloading = true;
                    await this.Dispatcher.RunAsync(() =>
                    {
                        this.BindingModel.ShowProgressRing = true;
                        this.BindingModel.MessageText = "Downloading songs to local cache...";
                    });
                    break;
                }
                case SongCachingChangeEventType.FailedToDownload:
                {
                    this.isDownloading = false;
                    await this.Dispatcher.RunAsync(() =>
                    {
                        this.BindingModel.ShowProgressRing = false;
                        this.BindingModel.MessageText = "Error happened on download songs to local cache...";
                    });
                    break;
                }
                case SongCachingChangeEventType.ClearCache:
                case SongCachingChangeEventType.FinishDownloading:
                case SongCachingChangeEventType.DownloadCanceled:
                {
                    this.isDownloading = false;
                    await this.Dispatcher.RunAsync(() =>
                    {
                        this.BindingModel.ShowProgressRing = false;
                        this.SetOfflineMessageIfRequired();
                    });
                    break;
                }
            }
        }

        private async void SynchronizationTimerOnTick(object sender, object o)
        {
            await this.Synchronize();
        }

        private async Task Synchronize()
        {
            await this.dispatcher.RunAsync(() => this.synchronizationTimer.Stop());

            if (this.stateService.IsOnline() && !this.isDownloading)
            {
                this.disableClickToCache = true;

                await this.dispatcher.RunAsync(
                    () =>
                    {
                        this.BindingModel.ShowProgressRing = true;
                        this.BindingModel.MessageText = this.resources.GetString("LinksRegion_UpdatingSongs");
                    });

                bool error = false;

                try
                {
                    await this.googleMusicSynchronizationService.UpdateSongsAsync();

                    if (this.synchronizationTime == 0)
                    {
                        await this.dispatcher.RunAsync(() => { this.BindingModel.MessageText = this.resources.GetString("LinksRegion_UpdatingPlaylists"); });
                        await this.googleMusicSynchronizationService.UpdateUserPlaylistsAsync();
                    }
                }
                catch (Exception e)
                {
                    this.Logger.Error(e, "Exception while update user playlist.");
                    error = true;
                }

                await this.dispatcher.RunAsync(
                         () =>
                         {
                             this.BindingModel.ShowProgressRing = false;
                             this.BindingModel.MessageText = error ? this.resources.GetString("LinksRegion_FailedToUpdate") : this.resources.GetString("LinksRegion_Updated");
                         });
                await Task.Delay(TimeSpan.FromSeconds(2));

                this.disableClickToCache = false;
            }

            await this.dispatcher.RunAsync(
                     () =>
                     {
                         this.synchronizationTime++;
                         if (this.synchronizationTime >= 6)
                         {
                             this.synchronizationTime = 0;
                         }

                         this.synchronizationTimer.Start();
                         this.SetOfflineMessageIfRequired();
                     });
        }
    }
}