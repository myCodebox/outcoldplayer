// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;

    using Windows.Networking.Connectivity;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters.Popups;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    using Windows.ApplicationModel;
    using Windows.System;
    using Windows.UI.Popups;

    public class StartPageViewPresenter : PagePresenterBase<IStartPageView, StartViewBindingModel>
    {
        private const int MaxItems = 12;

        private const int AskForReviewStarts = 10;
        private const string DoNotAskToReviewKey = "DoNotAskToReviewKey";
        private const string CountOfStartsBeforeReview = "CountOfStartsBeforeReview";

        private readonly IApplicationResources resources;

        private readonly ISettingsService settingsService;
        private readonly IAuthentificationService authentificationService;
        private readonly IPlayQueueService playQueueService;
        private readonly INavigationService navigationService;
        private readonly IPlaylistsService playlistsService;
        private readonly IMainFrameRegionProvider mainFrameRegionProvider;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly ISearchService searchService;
        private readonly ISongsCachingService cachingService;
        private readonly IApplicationStateService stateService;

        private bool initialized = false;

        public StartPageViewPresenter(
            IApplicationResources resources,
            ISettingsService settingsService,
            IAuthentificationService authentificationService,
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            IPlaylistsService playlistsService,
            IMainFrameRegionProvider mainFrameRegionProvider,
            IGoogleMusicSessionService sessionService,
            ISearchService searchService,
            ISongsCachingService cachingService,
            IApplicationStateService stateService)
        {
            this.resources = resources;
            this.settingsService = settingsService;
            this.authentificationService = authentificationService;
            this.playQueueService = playQueueService;
            this.navigationService = navigationService;
            this.playlistsService = playlistsService;
            this.mainFrameRegionProvider = mainFrameRegionProvider;
            this.sessionService = sessionService;
            this.searchService = searchService;
            this.cachingService = cachingService;
            this.stateService = stateService;

            this.PlayCommand = new DelegateCommand(this.Play);
            this.QueueCommand = new DelegateCommand(this.Queue, () => this.BindingModel.SelectedItems.Count > 0);
            this.DownloadCommand = new DelegateCommand(this.Download, () => this.BindingModel.SelectedItems.Count > 0);
            this.UnPinCommand = new DelegateCommand(this.UnPin, () => this.BindingModel.SelectedItems.Count > 0);

            this.sessionService.SessionCleared += async (sender, args) => 
                    {
                        await this.DeinitializeAsync();
                        this.ShowAuthentificationPopupView();
                    };
        }

        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand QueueCommand { get; private set; }

        public DelegateCommand DownloadCommand { get; set; }

        public DelegateCommand UnPinCommand { get; set; }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            if (!this.initialized)
            {
                await this.InitializeOnFirstLaunchAsync();
            }
            else
            {
                await this.LoadGroupsAsync();
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.SelectedItems.CollectionChanged += this.SelectedItemsOnCollectionChanged;

            this.EventAggregator.GetEvent<ReloadSongsEvent>()
                .Subscribe(async (e) =>
                {
                    await this.DeinitializeAsync();
                    this.ShowProgressLoadingPopupView(forceToRefreshDb: true);
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
                bool fCurrentVersion = string.Equals(currentVersion, Package.Current.Id.Version.ToVersionString(), StringComparison.OrdinalIgnoreCase);

                if (fCurrentVersion)
                {
                    await this.OnViewInitializedAsync();
                }
                else
                {
                    this.ShowProgressLoadingPopupView(forceToRefreshDb: false);
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
            this.ShowProgressLoadingPopupView(forceToRefreshDb: true);
        }

        private void ShowProgressLoadingPopupView(bool forceToRefreshDb)
        {
            this.Dispatcher.RunAsync(
                () =>
                    {
                        this.MainFrame.ShowPopup<IProgressLoadingPopupView>(PopupRegion.Full, new ProgressLoadingPopupViewRequest(forceToRefreshDb)).Closed += this.ProgressLoadingPopupView_Closed;
                    });
        }

        private async void ProgressLoadingPopupView_Closed(object sender, EventArgs eventArgs)
        {
            ((IProgressLoadingPopupView)sender).Closed -= this.ProgressLoadingPopupView_Closed;

            var progressLoadingCloseEventArgs = eventArgs as ProgressLoadingCloseEventArgs;
            if (progressLoadingCloseEventArgs != null && progressLoadingCloseEventArgs.IsFailed)
            {
                this.sessionService.ClearSession();
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
            await this.cachingService.ClearCacheAsync();
            await this.Dispatcher.RunAsync(
                () =>
                {
                    this.searchService.Unregister();
                    this.mainFrameRegionProvider.SetContent(MainFrameRegion.Links, null);
                    this.BindingModel.ClearSelectedItems();
                    this.BindingModel.Groups = null;
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
                    var networkConnectivityLevel = NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel();
                    if (networkConnectivityLevel != NetworkConnectivityLevel.ConstrainedInternetAccess
                        && networkConnectivityLevel != NetworkConnectivityLevel.InternetAccess)
                    {
                        this.stateService.CurrentState = ApplicationState.Offline;
                    }
                }

                await this.Dispatcher.RunAsync(() =>
                    {
                        this.searchService.Register();
                        this.mainFrameRegionProvider.SetContent(MainFrameRegion.Links, ApplicationBase.Container.Resolve<LinksRegionView>());
                    });

                this.cachingService.StartDownloadTask();

                this.initialized = true;
            }

            bool loadFailed = false;

            try
            {
                await this.LoadGroupsAsync();
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

        private async Task LoadGroupsAsync()
        {
            var types = new[]
                            {
                                PlaylistType.SystemPlaylist, PlaylistType.UserPlaylist, PlaylistType.Artist, PlaylistType.Album,
                                PlaylistType.Genre
                            };

            var groups = await Task.WhenAll(
                types.Select(
                    (t) => Task.Run(
                        async () =>
                            {
                                var countTask = this.playlistsService.GetCountAsync(t);
                                var getAllTask = this.playlistsService.GetAllAsync(t, Order.LastPlayed, MaxItems);

                                await Task.WhenAll(countTask, getAllTask);

                                int count = await countTask;
                                IEnumerable<IPlaylist> playlists = await getAllTask;

                                return this.CreateGroup(this.resources.GetPluralTitle(t), count, playlists, t);
                            })));

            await this.Dispatcher.RunAsync(() => { this.BindingModel.Groups = groups.Where(g => g.Playlists.Count > 0).ToList(); });
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnSelectedItemsChanged();
        }

        private void OnSelectedItemsChanged()
        {
            if (this.BindingModel.SelectedItems.Count > 0)
            {
                this.MainFrame.SetContextCommands(this.GetContextCommands());
            }
            else
            {
                this.MainFrame.ClearContextCommands();
            }
        }

        private IEnumerable<CommandMetadata> GetContextCommands()
        {
            yield return new CommandMetadata(CommandIcon.OpenWith, this.resources.GetString("Toolbar_QueueButton"), this.QueueCommand);

            if (this.BindingModel.SelectedItems.Any(x => x.Playlist.OfflineSongsCount != x.Playlist.SongsCount))
            {
                if (this.stateService.IsOnline())
                {
                    yield return new CommandMetadata(CommandIcon.Pin, this.resources.GetString("Toolbar_KeepLocal"), this.DownloadCommand);
                }
            }
            else
            {
                yield return new CommandMetadata(CommandIcon.UnPin, this.resources.GetString("Toolbar_RemoveLocal"), this.UnPinCommand);
            }
        }

        private PlaylistsGroupBindingModel CreateGroup(string title, int playlistsCount, IEnumerable<IPlaylist> playlists, PlaylistType type)
        {
            List<PlaylistBindingModel> groupItems =
                playlists.Select(
                    playlist =>
                    new PlaylistBindingModel(playlist)
                        {
                            PlayCommand = this.PlayCommand
                        }).ToList();

            return new PlaylistsGroupBindingModel(
                title,
                playlistsCount,
                groupItems,
                type);
        }

        private void Play(object commandParameter)
        {
            IPlaylist playlist = commandParameter as IPlaylist;
            if (playlist != null)
            {
                this.MainFrame.IsBottomAppBarOpen = true;
                this.Logger.LogTask(this.playQueueService.PlayAsync(playlist));
                this.navigationService.NavigateTo<ICurrentPlaylistPageView>();
            }
        }

        private void Queue()
        {
            this.MainFrame.ShowPopup<IQueueActionsPopupView>(
                PopupRegion.AppToolBarLeft,
                new SelectedItems(this.BindingModel.SelectedItems.Select(bm => bm.Playlist).ToList())).Closed += this.QueueActionsPopupView_Closed;
        }

        private void QueueActionsPopupView_Closed(object sender, EventArgs eventArgs)
        {
            ((IPopupView)sender).Closed -= this.QueueActionsPopupView_Closed;
            if (eventArgs is QueueActionsCompletedEventArgs)
            {
                this.BindingModel.ClearSelectedItems();
            }
        }

        private async void Download()
        {
            try
            {
                IEnumerable<Song> songs = Enumerable.Empty<Song>();

                foreach (var playlistBindingModel in this.BindingModel.SelectedItems)
                {
                    songs = songs.Union(await this.playlistsService.GetSongsAsync(playlistBindingModel.Playlist));
                }

                await this.cachingService.QueueForDownloadAsync(songs);
                this.BindingModel.ClearSelectedItems();
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Cannot add songs to download queue");
            }
        }

        private async void UnPin()
        {
            try
            {
                IEnumerable<Song> songs = Enumerable.Empty<Song>();

                foreach (var playlistBindingModel in this.BindingModel.SelectedItems)
                {
                    songs = songs.Union(await this.playlistsService.GetSongsAsync(playlistBindingModel.Playlist));
                }

                await this.cachingService.ClearCachedAsync(songs);
                this.BindingModel.ClearSelectedItems();
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Cannot remove from cache selected songs.");
            }
        }

        private void VerifyIfCanAskForReview()
        {
            bool dontAsk = this.settingsService.GetRoamingValue<bool>(DoNotAskToReviewKey);
            if (!dontAsk)
            {
                int startsCount = this.settingsService.GetRoamingValue<int>(CountOfStartsBeforeReview);
                if (startsCount >= AskForReviewStarts)
                {
                    this.Logger.LogTask(this.VerifyToReview());
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