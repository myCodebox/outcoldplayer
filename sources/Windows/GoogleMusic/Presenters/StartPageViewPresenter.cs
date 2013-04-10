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

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    using Windows.System;
    using Windows.UI.Popups;

    public class StartPageViewPresenter : PagePresenterBase<IStartPageView, StartViewBindingModel>
    {
        private const int MaxItems = 12;

        private const int AskForReviewStarts = 10;
        private const string DoNotAskToReviewKey = "DoNotAskToReviewKey";
        private const string CountOfStartsBeforeReview = "CountOfStartsBeforeReview";

        private const string CurrentVersion = "2.0.0.3";

        private readonly ISettingsService settingsService;
        private readonly IAuthentificationService authentificationService;
        private readonly IPlayQueueService playQueueService;
        private readonly INavigationService navigationService;
        private readonly IPlaylistsService playlistsService;
        private readonly IMainFrameRegionProvider mainFrameRegionProvider;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly ISearchService searchService;

        private bool initialized = false;

        public StartPageViewPresenter(
            ISettingsService settingsService,
            IAuthentificationService authentificationService,
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            IPlaylistsService playlistsService,
            IMainFrameRegionProvider mainFrameRegionProvider,
            IGoogleMusicSessionService sessionService,
            ISearchService searchService)
        {
            this.settingsService = settingsService;
            this.authentificationService = authentificationService;
            this.playQueueService = playQueueService;
            this.navigationService = navigationService;
            this.playlistsService = playlistsService;
            this.mainFrameRegionProvider = mainFrameRegionProvider;
            this.sessionService = sessionService;
            this.searchService = searchService;

            this.PlayCommand = new DelegateCommand(this.Play);
            this.QueueCommand = new DelegateCommand(this.Queue, () => this.BindingModel.SelectedItems.Count > 0);

            this.sessionService.SessionCleared += async (sender, args) => 
                    {
                        await this.DeinitializeAsync();
                        this.ShowAuthentificationPopupView();
                    };
        }

        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand QueueCommand { get; private set; }

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
                bool fCurrentVersion = string.Equals(currentVersion, CurrentVersion, StringComparison.OrdinalIgnoreCase);

                if (fCurrentVersion)
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
            ((IAuthentificationPopupView)sender).Closed += this.AuthentificationPopupView_Closed;
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
            var currentVersion = this.settingsService.GetValue<string>("Version", null);
            bool fCurrentVersion = string.Equals(currentVersion, CurrentVersion, StringComparison.OrdinalIgnoreCase);
            bool fUpdate = !fCurrentVersion && currentVersion != null;

            ((IProgressLoadingPopupView)sender).Closed += this.AuthentificationPopupView_Closed;
            await this.OnViewInitializedAsync();

            this.settingsService.SetValue("Version", CurrentVersion);

            if (fUpdate)
            {
                this.MainFrame.ShowPopup<IReleasesHistoryPopupView>(PopupRegion.Full);
            }

            this.VerifyIfCanAskForReview();
        }

        private async Task DeinitializeAsync()
        {
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
                await this.Dispatcher.RunAsync(() =>
                    {
                        this.searchService.Register();
                        this.mainFrameRegionProvider.SetContent(MainFrameRegion.Links, ApplicationBase.Container.Resolve<LinksRegionView>());
                    });

                this.initialized = true;
            }

            try
            {
                await this.LoadGroupsAsync();
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Cannot load groups");
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

                                return this.CreateGroup(t.ToPluralTitle(), count, playlists, t);
                            })));

            await this.Dispatcher.RunAsync(() => { this.BindingModel.Groups = groups.ToList(); });
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
            yield return new CommandMetadata(CommandIcon.OpenWith, "Queue", this.QueueCommand);
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
                this.navigationService.NavigateToPlaylist(playlist);
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
            var dialog = new MessageDialog("If you are enjoy using gMusic appication, would you mind taking a moment to rate it? Good ratings help us a lot. It won't take more than a minute. Thanks for your support!");
            dialog.Commands.Add(
                new UICommand(
                    "Rate",
                    (cmd) =>
                    {
                        this.settingsService.SetRoamingValue<bool>(DoNotAskToReviewKey, true);
                        this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=47286outcoldman.gMusic_z1q2m7teapq4y")).AsTask());
                    }));

            dialog.Commands.Add(
                new UICommand(
                    "No, thanks",
                    (cmd) =>
                    this.settingsService.SetRoamingValue<bool>(DoNotAskToReviewKey, true)));

            dialog.Commands.Add(
                new UICommand(
                    "Remind me later",
                    (cmd) =>
                    this.settingsService.SetRoamingValue<int>(CountOfStartsBeforeReview, 0)));

            return dialog.ShowAsync().AsTask();
        }
    }
}