// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;

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

        private readonly IAnalyticsService analyticsService;

        private readonly DispatcherTimer synchronizationTimer;

        private bool isDownloading = false;
        private bool disableClickToCache = false;

        private bool showProgressRing;
        private string messageText;

        public LinksRegionViewPresenter(
            IApplicationStateService stateService,
            IApplicationResources resources,
            IDispatcher dispatcher,
            IGoogleMusicSynchronizationService googleMusicSynchronizationService,
            IApplicationSettingViewsService applicationSettingViewsService,
            IGoogleMusicSessionService sessionService,
            INavigationService navigationService,
            IAnalyticsService analyticsService)
        {
            this.stateService = stateService;
            this.resources = resources;
            this.dispatcher = dispatcher;
            this.googleMusicSynchronizationService = googleMusicSynchronizationService;
            this.sessionService = sessionService;
            this.navigationService = navigationService;
            this.analyticsService = analyticsService;
            this.NavigateToDownloadQueue = new DelegateCommand(async () =>
            {
                if (!this.IsOnline)
                {
                    this.SwitchModeCommand.Execute();
                } 
                else if (!this.disableClickToCache)
                {
                    this.analyticsService.SendEvent("LinksRegion", "Execute", "NavigatToDownload");

                    await this.dispatcher.RunAsync(
                        () => applicationSettingViewsService.Show("offlinecache"));
                }
            });

            this.UpdateLibraryCommand = new DelegateCommand(
                async () =>
                    {
                        this.analyticsService.SendEvent("LinksRegion", "Execute", "UpdateLibrary");

                        if (this.UpdateLibraryCommand.CanExecute())
                        {
                            this.synchronizationTimer.Stop();
                            await this.Synchronize();
                        }
                    },
                () => !this.ShowProgressRing);

            this.GetHelpCommand = new DelegateCommand(
                () =>
                {
                    this.analyticsService.SendEvent("LinksRegion", "Execute", "GetHelp");
                    applicationSettingViewsService.Show("support");
                });

            this.synchronizationTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            this.synchronizationTimer.Stop();

            this.synchronizationTimer.Tick += this.SynchronizationTimerOnTick;

            this.sessionService.SessionCleared += this.SessionServiceOnSessionCleared;

            this.SwitchModeCommand = new DelegateCommand(
                () =>
                {
                    this.IsOnline = !this.IsOnline;
                    this.analyticsService.SendEvent("LinksRegion", "Execute", "SwitchMode to " + (this.IsOnline ? "Online" : "Offline"));
                });
        }

        public DelegateCommand NavigateToDownloadQueue { get; private set; }

        public DelegateCommand UpdateLibraryCommand { get; private set; }

        public DelegateCommand SwitchModeCommand { get; set; }

        public DelegateCommand GetHelpCommand { get; set; }

        public bool ShowProgressRing
        {
            get
            {
                return this.showProgressRing;
            }

            set
            {
                this.SetValue(ref this.showProgressRing, value);
            }
        }

        public string MessageText
        {
            get
            {
                return this.messageText;
            }

            set
            {
                this.SetValue(ref this.messageText, value);
            }
        }

        public bool IsOnline
        {
            get
            {
                return this.stateService.CurrentState == ApplicationState.Online;
            }

            set
            {
                this.stateService.CurrentState = value ? ApplicationState.Online : ApplicationState.Offline;
                this.RaiseCurrentPropertyChanged();
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.EventAggregator.GetEvent<CachingChangeEvent>()
                                .Subscribe(async e => await this.dispatcher.RunAsync(() => this.OnCachingEvent(e.EventType)));

            this.EventAggregator.GetEvent<ApplicationStateChangeEvent>()
                                .Subscribe(async e => await this.dispatcher.RunAsync(this.SetOfflineMessageIfRequired));

            this.SetOfflineMessageIfRequired();

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
                this.ShowProgressRing = false;
                this.MessageText = "Offline mode";
            }
            else
            {
                this.ShowProgressRing = false;
                this.MessageText = null;
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
                        this.ShowProgressRing = true;
                        this.MessageText = "Downloading songs...";
                    });
                    break;
                }
                case SongCachingChangeEventType.FailedToDownload:
                {
                    this.isDownloading = false;
                    await this.Dispatcher.RunAsync(() =>
                    {
                        this.ShowProgressRing = false;
                        this.MessageText = "Error happened...";
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
                        this.ShowProgressRing = false;
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
                            this.ShowProgressRing = true;
                            this.MessageText = this.resources.GetString("LinksRegion_UpdatingSongs");
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
                                 this.ShowProgressRing = false;
                                 this.MessageText = error ? this.resources.GetString("LinksRegion_FailedToUpdate") : this.resources.GetString("LinksRegion_Updated");
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