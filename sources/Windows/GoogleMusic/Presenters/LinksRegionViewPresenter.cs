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

    using Windows.UI.Popups;
    using Windows.UI.Xaml;

    public class LinksRegionViewPresenter : ViewPresenterBase<IView>
    {
        private readonly IApplicationStateService stateService;
        private readonly IApplicationResources resources;
        private readonly IDispatcher dispatcher;
        private readonly IGoogleMusicSynchronizationService googleMusicSynchronizationService;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly INavigationService navigationService;

        private readonly DispatcherTimer synchronizationTimer;

        private bool isDownloading = false;
        private bool disableClickToCache = false;

        public LinksRegionViewPresenter(
            IApplicationStateService stateService,
            IApplicationResources resources,
            ISearchService searchService,
            IDispatcher dispatcher,
            IGoogleMusicSynchronizationService googleMusicSynchronizationService,
            IApplicationSettingViewsService applicationSettingViewsService,
            IGoogleMusicSessionService sessionService,
            INavigationService navigationService)
        {
            this.stateService = stateService;
            this.resources = resources;
            this.dispatcher = dispatcher;
            this.googleMusicSynchronizationService = googleMusicSynchronizationService;
            this.sessionService = sessionService;
            this.navigationService = navigationService;
            this.ShowSearchCommand = new DelegateCommand(searchService.Activate);
            this.NavigateToDownloadQueue = new DelegateCommand(async () =>
            {
                if (!this.disableClickToCache)
                {
                    await this.dispatcher.RunAsync(() => applicationSettingViewsService.Show("offlinecache"));
                }
            });

            this.UpdateLibraryCommand = new DelegateCommand(
                async () =>
                    {
                        if (this.UpdateLibraryCommand.CanExecute())
                        {
                            this.synchronizationTimer.Stop();
                            await this.Synchronize();
                        }
                    },
                () => !this.BindingModel.ShowProgressRing);

            this.BindingModel = new LinksRegionBindingModel();

            this.synchronizationTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            this.synchronizationTimer.Stop();

            this.synchronizationTimer.Tick += this.SynchronizationTimerOnTick;

            this.SetOfflineMessageIfRequired();

            this.sessionService.SessionCleared += this.SessionServiceOnSessionCleared;
        }

        public DelegateCommand ShowSearchCommand { get; private set; }

        public DelegateCommand NavigateToDownloadQueue { get; private set; }

        public DelegateCommand UpdateLibraryCommand { get; private set; }

        public LinksRegionBindingModel BindingModel { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.EventAggregator.GetEvent<CachingChangeEvent>()
                                .Subscribe(async e => await this.dispatcher.RunAsync(() => this.OnCachingEvent(e.EventType)));

            this.EventAggregator.GetEvent<ApplicationStateChangeEvent>()
                                .Subscribe(async e => await this.dispatcher.RunAsync(this.SetOfflineMessageIfRequired));

            this.Logger.LogTask(this.Synchronize());
        }

        private void SessionServiceOnSessionCleared(object sender, EventArgs eventArgs)
        {
            this.synchronizationTimer.Stop();
            this.sessionService.SessionCleared -= this.SessionServiceOnSessionCleared;
        }

        private void SetOfflineMessageIfRequired()
        {
            this.isDownloading = false;

            if (this.stateService.IsOffline())
            {
                this.BindingModel.ShowProgressRing = false;
                this.BindingModel.MessageText = "Offline mode (listen only)";
            }
            else
            {
                this.BindingModel.ShowProgressRing = false;
                this.BindingModel.MessageText = null;
            }

            this.UpdateLibraryCommand.RaiseCanExecuteChanged();
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
                        this.UpdateLibraryCommand.RaiseCanExecuteChanged();
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
                        this.UpdateLibraryCommand.RaiseCanExecuteChanged();
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
            UpdateStatus updateStatus = null;

            await this.dispatcher.RunAsync(() => this.synchronizationTimer.Stop());

            if (this.stateService.IsOnline() && !this.isDownloading)
            {
                await this.dispatcher.RunAsync(
                    () =>
                    {
                        if (this.stateService.IsOnline())
                        {
                            this.disableClickToCache = true;
                            this.BindingModel.ShowProgressRing = true;
                            this.BindingModel.MessageText = this.resources.GetString("LinksRegion_UpdatingSongs");
                            this.UpdateLibraryCommand.RaiseCanExecuteChanged();
                        }
                    });

                bool error = false;

                try
                {
                    updateStatus = await this.googleMusicSynchronizationService.Update();
                }
                catch (Exception e)
                {
                    this.Logger.Error(e, "Exception while update user playlist.");
                    error = true;
                }

                await this.dispatcher.RunAsync(
                         () =>
                         {
                             this.disableClickToCache = false;

                             if (this.stateService.IsOnline())
                             {
                                 this.BindingModel.ShowProgressRing = false;
                                 this.BindingModel.MessageText = error ? this.resources.GetString("LinksRegion_FailedToUpdate") : this.resources.GetString("LinksRegion_Updated");
                             }
                         });

                if (updateStatus != null)
                {
                    this.ShowUpdateMessage(updateStatus);
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            await this.dispatcher.RunAsync(
                     () =>
                     {
                         this.synchronizationTimer.Start();
                         this.SetOfflineMessageIfRequired();
                         this.UpdateLibraryCommand.RaiseCanExecuteChanged();
                     });
        }

        private async void ShowUpdateMessage(
            UpdateStatus updateStatus)
        {
            if (updateStatus != null && updateStatus.IsBreakingChange)
            {
                var dialog = new MessageDialog(this.resources.GetString("Update_MessageBox_Updates_Message"));
                dialog.Commands.Add(
                    new UICommand(
                        this.resources.GetString("Update_MessageBox_Updates_OkButton"),
                        (cmd) =>
                            {
                                this.navigationService.ClearHistory();
                                this.navigationService.RefreshCurrentView();
                            }));

                dialog.Commands.Add(new UICommand(this.resources.GetString("Update_MessageBox_Updates_CancelButton")));

                try
                {
                    await dialog.ShowAsync().AsTask();
                }
                catch (Exception e)
                {
                    this.Logger.Error(e, "Could not show message dialog: ShowUpdateMessage");
                }
            }
        }
    }
}