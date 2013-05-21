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

    using Windows.System;
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
                            await this.Synchronize(forceToDownloadPlaylists: true);
                        }
                    },
                () => !this.BindingModel.ShowProgressRing);

            this.BindingModel = new LinksRegionBindingModel();

            this.synchronizationTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            this.synchronizationTimer.Stop();
            this.synchronizationTime = 0;

            this.synchronizationTimer.Tick += this.SynchronizationTimerOnTick;

            this.Logger.LogTask(this.Synchronize());

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

        private async Task Synchronize(bool forceToDownloadPlaylists = false)
        {
            SongsUpdateStatus songsUpdateStatus = null;
            UserPlaylistsUpdateStatus userPlaylistsUpdateStatus = null;

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
                    songsUpdateStatus = await this.googleMusicSynchronizationService.UpdateSongsAsync();

                    if (this.synchronizationTime == 0 || forceToDownloadPlaylists)
                    {
                        await this.dispatcher.RunAsync(() => { this.BindingModel.MessageText = this.resources.GetString("LinksRegion_UpdatingPlaylists"); });
                        userPlaylistsUpdateStatus = await this.googleMusicSynchronizationService.UpdateUserPlaylistsAsync();
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
                             this.disableClickToCache = false;

                             if (this.stateService.IsOnline())
                             {
                                 this.BindingModel.ShowProgressRing = false;
                                 this.BindingModel.MessageText = error ? this.resources.GetString("LinksRegion_FailedToUpdate") : this.resources.GetString("LinksRegion_Updated");
                             }
                         });

                this.ShowUpdateMessage(userPlaylistsUpdateStatus, songsUpdateStatus);

                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            await this.dispatcher.RunAsync(
                     () =>
                     {
                         this.synchronizationTime++;
                         if (forceToDownloadPlaylists)
                         {
                             this.synchronizationTime = 1;
                         }
                         else if (this.synchronizationTime >= 6)
                         {
                             this.synchronizationTime = 0;
                         }

                         this.synchronizationTimer.Start();
                         this.SetOfflineMessageIfRequired();
                         this.UpdateLibraryCommand.RaiseCanExecuteChanged();
                     });
        }

        private async void ShowUpdateMessage(
            UserPlaylistsUpdateStatus userPlaylistsUpdateStatus, SongsUpdateStatus songsUpdateStatus)
        {
            if ((userPlaylistsUpdateStatus != null
                && (userPlaylistsUpdateStatus.DeletedPlaylists + userPlaylistsUpdateStatus.UpdatedPlaylists + userPlaylistsUpdateStatus.NewPlaylists) > 0)
                || (songsUpdateStatus != null
                && (songsUpdateStatus.DeletedSongs + songsUpdateStatus.UpdatedSongs + songsUpdateStatus.NewSongs) > 0))
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

                await dialog.ShowAsync().AsTask();
            }
        }
    }
}