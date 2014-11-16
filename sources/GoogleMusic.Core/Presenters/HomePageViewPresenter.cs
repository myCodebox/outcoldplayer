// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters.Popups;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class HomePageViewPresenter : PlaylistsPageViewPresenterBase<IHomePageView, HomePageViewBindingModel>
    {
        private readonly IShellService shellService;
        private readonly ISettingsService settingsService;
        private readonly IAuthentificationService authentificationService;
        private readonly INavigationService navigationService;
        private readonly IPlaylistsService playlistsService;
        private readonly IMainFrameRegionProvider mainFrameRegionProvider;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly ISongsCachingService cachingService;
        private readonly IApplicationStateService stateService;
        private readonly IAllAccessService allAccessService;

        private bool initialized = false;

        public HomePageViewPresenter(
            IShellService shellService,
            ISettingsService settingsService,
            IAuthentificationService authentificationService,
            INavigationService navigationService,
            IPlaylistsService playlistsService,
            IMainFrameRegionProvider mainFrameRegionProvider,
            IGoogleMusicSessionService sessionService,
            ISongsCachingService cachingService,
            IApplicationStateService stateService,
            IAllAccessService allAccessService)
            : base(playlistsService)
        {
            this.shellService = shellService;
            this.settingsService = settingsService;
            this.authentificationService = authentificationService;
            this.navigationService = navigationService;
            this.playlistsService = playlistsService;
            this.mainFrameRegionProvider = mainFrameRegionProvider;
            this.sessionService = sessionService;
            this.cachingService = cachingService;
            this.stateService = stateService;
            this.allAccessService = allAccessService;

            this.sessionService.SessionCleared += async (sender, args) => 
                    {
                        await this.DeinitializeAsync();
                        this.ShowAuthentificationPopupView();
                    };

            this.IsMixedList = true;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs, CancellationToken cancellationToken)
        {
            this.BindingModel.PlaylistType = PlaylistType.Unknown;
            this.BindingModel.Title = "Home";

            if (!this.initialized)
            {
                await this.InitializeOnFirstLaunchAsync();
            }
            else
            {
                await this.LoadPlaylists(cancellationToken);
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.EventAggregator.GetEvent<ReloadSongsEvent>()
                .Subscribe(async (e) =>
                {
                    await this.DeinitializeAsync();
                    this.ShowProgressLoadingPopupView();
                });
        }

        private async Task InitializeOnFirstLaunchAsync()
        {
            AuthentificationService.AuthentificationResult result = null;

            try
            {
                result = await this.authentificationService.CheckAuthentificationAsync();
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Exception while tried to verifying authentification.");
            }

            if (result != null && result.Succeed)
            {
                var currentVersion = this.settingsService.GetValue<string>("Version", null);
                var dbContext = new DbContext();
                bool fCurrentVersion = string.Equals(currentVersion, ApplicationContext.ApplicationVersion.ToVersionString(), StringComparison.OrdinalIgnoreCase);

                if (fCurrentVersion && await dbContext.CheckVersionAsync())
                {
                    await this.OnViewInitializedAsync();
                }
                else
                {
                    this.ShowProgressLoadingPopupView();
                }
            }
            else
            {
                this.ShowAuthentificationPopupView();
            }
        }

        private void ShowAuthentificationPopupView()
        {
            this.Dispatcher.RunAsync(
                () =>
                    {
                        this.MainFrame.ShowPopup<IAuthentificationPopupView>(PopupRegion.Full).Closed += this.AuthentificationPopupView_Closed;
                    });
        }

        private void AuthentificationPopupView_Closed(object sender, EventArgs eventArgs)
        {
            ((IAuthentificationPopupView)sender).Closed -= this.AuthentificationPopupView_Closed;
            this.ShowProgressLoadingPopupView();
        }

        private void ShowProgressLoadingPopupView()
        {
            this.Dispatcher.RunAsync(
                () =>
                    {
                        this.MainFrame.ShowPopup<IProgressLoadingPopupView>(PopupRegion.Full).Closed += this.ProgressLoadingPopupView_Closed;
                    });
        }

        private async void ProgressLoadingPopupView_Closed(object sender, EventArgs eventArgs)
        {
            ((IProgressLoadingPopupView)sender).Closed -= this.ProgressLoadingPopupView_Closed;

            var progressLoadingCloseEventArgs = eventArgs as ProgressLoadingCloseEventArgs;
            if (progressLoadingCloseEventArgs != null && progressLoadingCloseEventArgs.IsFailed)
            {
                await this.sessionService.ClearSession();
                return;
            }

            var currentVersion = this.settingsService.GetValue<string>("Version", null);
            bool fCurrentVersion = string.Equals(currentVersion, ApplicationContext.ApplicationVersion.ToVersionString(), StringComparison.OrdinalIgnoreCase);
            bool fUpdate = !fCurrentVersion && currentVersion != null;
            
            await this.OnViewInitializedAsync();

            this.settingsService.SetValue("Version", ApplicationContext.ApplicationVersion.ToVersionString());

            if (fUpdate)
            {
                this.MainFrame.ShowPopup<IReleasesHistoryPopupView>(PopupRegion.Full).Closed += ReleasesHistoryPopupView_OnClosed;
            }
        }

        private void ReleasesHistoryPopupView_OnClosed(object sender, EventArgs eventArgs)
        {
            ((IReleasesHistoryPopupView)(sender)).Closed -= this.ReleasesHistoryPopupView_OnClosed;
            if (!this.settingsService.GetRoamingValue("Tutorial-v3", false))
            {
                this.MainFrame.ShowPopup<ITutorialPopupView>(PopupRegion.Full);
                this.settingsService.SetRoamingValue("Tutorial-v3", true);
            }
        }

        private async Task DeinitializeAsync()
        {
            try
            {
                await this.cachingService.ClearCacheAsync();
            }
            catch (Exception e)
            {
                this.Logger.Debug("Could not clear cache", e);
            }

            await this.Dispatcher.RunAsync(
                () =>
                {
                    this.mainFrameRegionProvider.SetContent(MainFrameRegion.Links, null);
                    this.BindingModel.Playlists = null;
                    this.navigationService.ClearHistory();
                    this.initialized = false;
                });
        }

        private async Task OnViewInitializedAsync()
        {
            if (!this.initialized)
            {
                if (this.stateService.CurrentState == ApplicationState.Online)
                {
                    if (!(await this.shellService.HasNetworkConnectionAsync()))
                    {
                        this.stateService.CurrentState = ApplicationState.Offline;
                        this.MainFrame.ShowMessage("Switched to offline mode...");
                    }
                }

                this.EventAggregator.Publish(new ApplicationInitializedEvent());

                this.initialized = true;
            }

            bool loadFailed = false;

            try
            {
                await this.LoadPlaylists(new CancellationToken());
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Cannot load groups");
                loadFailed = true;
            }

            if (loadFailed)
            {
                await this.DeinitializeAsync();
                this.ShowAuthentificationPopupView();
            }
        }

        private async Task LoadPlaylists(CancellationToken cancellationToken)
        {
            var systemPlaylists = new List<IPlaylist>();
            var localPlaylists = new List<IPlaylist>();

            var taskLoadLocal = Task.Run(async () =>
            {
                const int MaxItems = 18;

                List<IPlaylist> allPlaylists = new List<IPlaylist>();

                if (this.stateService.IsOnline())
                {
                    IList<IPlaylist> radioPlaylists = (await this.playlistsService.GetAllAsync(PlaylistType.Radio, Order.LastPlayed, MaxItems)).ToList();

                    systemPlaylists.Add(radioPlaylists.First());
                    allPlaylists.AddRange(radioPlaylists.Skip(1));
                }

                systemPlaylists.AddRange(
                    await this.playlistsService.GetAllAsync(PlaylistType.SystemPlaylist, Order.LastPlayed, MaxItems));

                foreach (var playlistType in new[] {PlaylistType.UserPlaylist, PlaylistType.Album, PlaylistType.Genre})
                {
                    allPlaylists.AddRange(
                        await this.playlistsService.GetAllAsync(playlistType, Order.LastPlayed, MaxItems));
                }

                localPlaylists.AddRange(allPlaylists.OrderByDescending(
                    x =>
                    {
                        var userPlaylist = x as UserPlaylist;
                        if (userPlaylist != null)
                        {
                            return userPlaylist.Recent > userPlaylist.CreationDate
                                ? userPlaylist.Recent
                                : userPlaylist.CreationDate;
                        }

                        return x.Recent;
                    }).Take(MaxItems));
            }, cancellationToken);

            var taskLoadSituation = this.stateService.IsOnline() && this.settingsService.GetIsAllAccessAvailable() ? this.allAccessService.GetSituationsAsync(cancellationToken) : Task.Run(() => (SituationsGroup)null, cancellationToken);

            await Task.WhenAll(taskLoadLocal, taskLoadSituation);

            SituationsGroup situationsGroup = await taskLoadSituation;
            await taskLoadLocal;

            await this.Dispatcher.RunAsync(
                () =>
                {
                    this.BindingModel.Playlists = localPlaylists;
                    this.BindingModel.SystemPlaylists = systemPlaylists;
                    this.BindingModel.SituationHeader = situationsGroup == null ? null : situationsGroup.Header;
                    this.BindingModel.Situations = situationsGroup == null ? null : situationsGroup.Situations;
                });
        }
    }
}