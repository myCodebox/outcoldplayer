// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters.Popups;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    using Windows.ApplicationModel;
    using Windows.Networking.Connectivity;
    using Windows.System;
    using Windows.UI.Popups;

    public class StartPageViewPresenter : PlaylistsPageViewPresenterBase<IStartPageView, PlaylistsPageViewBindingModel>
    {
        private const int MaxItems = 5;

        private const int AskForReviewStarts = 10;
        private const string DoNotAskToReviewKey = "DoNotAskToReviewKey";
        private const string CountOfStartsBeforeReview = "CountOfStartsBeforeReview";

        private readonly IApplicationResources resources;

        private readonly ISettingsService settingsService;
        private readonly IAuthentificationService authentificationService;
        private readonly INavigationService navigationService;
        private readonly IPlaylistsService playlistsService;
        private readonly IMainFrameRegionProvider mainFrameRegionProvider;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly ISongsCachingService cachingService;
        private readonly IApplicationStateService stateService;

        private bool initialized = false;

        public StartPageViewPresenter(
            IApplicationResources resources,
            ISettingsService settingsService,
            IAuthentificationService authentificationService,
            INavigationService navigationService,
            IPlaylistsService playlistsService,
            IMainFrameRegionProvider mainFrameRegionProvider,
            IGoogleMusicSessionService sessionService,
            ISongsCachingService cachingService,
            IApplicationStateService stateService)
            : base(resources, playlistsService)
        {
            this.resources = resources;
            this.settingsService = settingsService;
            this.authentificationService = authentificationService;
            this.navigationService = navigationService;
            this.playlistsService = playlistsService;
            this.mainFrameRegionProvider = mainFrameRegionProvider;
            this.sessionService = sessionService;
            this.cachingService = cachingService;
            this.stateService = stateService;

            this.sessionService.SessionCleared += async (sender, args) => 
                    {
                        await this.DeinitializeAsync();
                        this.ShowAuthentificationPopupView();
                    };
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            this.BindingModel.IsSemanticZoomEnabled = false;
            this.BindingModel.PlaylistType = PlaylistType.Unknown;
            this.BindingModel.Title = "Home";

            if (!this.initialized)
            {
                await this.InitializeOnFirstLaunchAsync();
            }
            else
            {
                await this.LoadPlaylists();
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
                bool fCurrentVersion = string.Equals(currentVersion, Package.Current.Id.Version.ToVersionString(), StringComparison.OrdinalIgnoreCase);

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
            bool fCurrentVersion = string.Equals(currentVersion, Package.Current.Id.Version.ToVersionString(), StringComparison.OrdinalIgnoreCase);
            bool fUpdate = !fCurrentVersion && currentVersion != null;
            
            await this.OnViewInitializedAsync();

            this.settingsService.SetValue("Version", Package.Current.Id.Version.ToVersionString());

            if (fUpdate)
            {
                this.MainFrame.ShowPopup<IReleasesHistoryPopupView>(PopupRegion.Full);
            }

            this.VerifyIfCanAskForReview();
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
                    var profile = NetworkInformation.GetInternetConnectionProfile();
                    if (profile != null)
                    {
                        var networkConnectivityLevel = profile.GetNetworkConnectivityLevel();
                        if (networkConnectivityLevel != NetworkConnectivityLevel.ConstrainedInternetAccess
                            && networkConnectivityLevel != NetworkConnectivityLevel.InternetAccess)
                        {
                            this.stateService.CurrentState = ApplicationState.Offline;
                        }
                    }
                }

                await this.Dispatcher.RunAsync(() => this.mainFrameRegionProvider.SetContent(MainFrameRegion.Links, ApplicationBase.Container.Resolve<LinksRegionView>()));

                this.cachingService.StartDownloadTask();

                this.initialized = true;
            }

            bool loadFailed = false;

            try
            {
                await this.LoadPlaylists();
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

        private async Task LoadPlaylists()
        {
            List<IPlaylist> results = new List<IPlaylist>();

            List<IPlaylist> allPlaylists = new List<IPlaylist>();

            if (this.stateService.IsOnline())
            {
                allPlaylists.AddRange(await this.playlistsService.GetAllAsync(PlaylistType.Radio, Order.LastPlayed, MaxItems));

                results.Add(allPlaylists[0]);
                allPlaylists.RemoveAt(0);
            }

            results.AddRange(await this.playlistsService.GetAllAsync(PlaylistType.SystemPlaylist, Order.LastPlayed, MaxItems));
            foreach (var playlistType in new[] { PlaylistType.UserPlaylist, PlaylistType.Artist, PlaylistType.Album, PlaylistType.Genre })
            {
                allPlaylists.AddRange(await this.playlistsService.GetAllAsync(playlistType, Order.LastPlayed, MaxItems));
            }

            results.AddRange(allPlaylists.OrderByDescending(x => x.Recent));
           

            await this.Dispatcher.RunAsync(() => { this.BindingModel.Playlists = results; });
        }

        private void VerifyIfCanAskForReview()
        {
            bool dontAsk = this.settingsService.GetRoamingValue<bool>(DoNotAskToReviewKey);
            if (!dontAsk)
            {
                int startsCount = this.settingsService.GetRoamingValue<int>(CountOfStartsBeforeReview);
                if (startsCount >= AskForReviewStarts)
                {
                    try
                    {
                        this.Logger.LogTask(this.VerifyToReview());
                    }
                    catch (Exception e) 
                    {
                        this.Logger.Error(e, "VerifyToReview failed");
                    }
                }
                else
                {
                    this.settingsService.SetRoamingValue<int>(CountOfStartsBeforeReview, startsCount + 1);
                }
            }
        }

        private Task VerifyToReview()
        {
            var dialog = new MessageDialog(this.resources.GetString("MessageBox_ReviewMessage"));
            dialog.Commands.Add(
                new UICommand(
                    this.resources.GetString("MessageBox_ReviewButtonRate"),
                    (cmd) =>
                    {
                        this.settingsService.SetRoamingValue<bool>(DoNotAskToReviewKey, true);
                        this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=47286outcoldman.gMusic_z1q2m7teapq4y")).AsTask());
                    }));

            dialog.Commands.Add(
                new UICommand(
                    this.resources.GetString("MessageBox_ReviewButtonNoThanks"),
                    (cmd) =>
                    this.settingsService.SetRoamingValue<bool>(DoNotAskToReviewKey, true)));

            dialog.Commands.Add(
                new UICommand(
                    this.resources.GetString("MessageBox_ReviewButtonRemind"),
                    (cmd) =>
                    this.settingsService.SetRoamingValue<int>(CountOfStartsBeforeReview, 0)));

            return dialog.ShowAsync().AsTask();
        }
    }
}