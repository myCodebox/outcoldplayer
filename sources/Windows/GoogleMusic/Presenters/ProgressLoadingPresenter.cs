// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Web;

    using Windows.System;
    using Windows.UI.Popups;

    public class ProgressLoadingPresenter : ViewPresenterBase<IView>
    {
        private const int AskForReviewStarts = 10;
        private const string DoNotAskToReviewKey = "DoNotAskToReviewKey";
        private const string CountOfStartsBeforeReview = "CountOfStartsBeforeReview";

        private readonly ISongsService songsService;
        private readonly IPlaylistsWebService playlistsWebService;
        private readonly INavigationService navigationService;

        private readonly ISearchService searchService;

        private readonly ISettingsService settingsService;

        public ProgressLoadingPresenter(
            IDependencyResolverContainer container, 
            IView view,
            ISongsService songsService,
            INavigationService navigationService,
            ISearchService searchService,
            ISettingsService settingsService,
            IPlaylistsWebService playlistsWebService)
            : base(container, view)
        {
            this.songsService = songsService;
            this.navigationService = navigationService;
            this.searchService = searchService;
            this.settingsService = settingsService;
            this.playlistsWebService = playlistsWebService;
            this.BindingModel = new ProgressLoadingBindingModel();
        }

        public ProgressLoadingBindingModel BindingModel { get; private set; }

        public override void OnNavigatedTo(object parameter)
        {
            base.OnNavigatedTo(parameter);

            this.LoadSongs();
        }

        public void LoadSongs()
        {
            this.BindingModel.Progress = 0;
            this.BindingModel.Message = "Initializing...";
            this.BindingModel.IsFailed = false;

            this.playlistsWebService.GetStatusAsync().ContinueWith(
                tStatus =>
                {
                    if (tStatus.IsCompleted)
                    {
                        this.BindingModel.Maximum = tStatus.Result.AvailableTracks;
                        this.BindingModel.Message = "Loading playlists...";

                        this.songsService.GetAllPlaylistsAsync().ContinueWith(
                            tPlaylists =>
                            {
                                if (tStatus.IsCompleted)
                                {
                                    this.BindingModel.Message = "Loading songs...";
                                    Progress<int> progress = new Progress<int>();
                                    progress.ProgressChanged += (sender, i) =>
                                    {
                                        this.BindingModel.Progress = i;
                                    };

                                    this.songsService.GetAllGoogleSongsAsync(progress).ContinueWith(
                                        tSongs =>
                                        {
                                            if (tSongs.IsCompleted)
                                            {
                                                bool dontAsk = this.settingsService.GetRoamingValue<bool>(DoNotAskToReviewKey);
                                                if (!dontAsk)
                                                {
                                                    int startsCount = this.settingsService.GetRoamingValue<int>(CountOfStartsBeforeReview);
                                                    if (startsCount >= AskForReviewStarts)
                                                    {
                                                        var dialog = new MessageDialog("If you are enjoy using gMusic appication, would you mind taking a moment to rate it? Good ratings help us a lot. It won't take more than a minute. Thanks for your support!");
                                                        dialog.Commands.Add(
                                                            new UICommand(
                                                                "Rate",
                                                                (cmd) =>
                                                                    {
                                                                        this.settingsService.SetRoamingValue<bool>(DoNotAskToReviewKey, true);
                                                                        var tLauncher = Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=47286outcoldman.gMusic_z1q2m7teapq4y"));
                                                                    }));
                                                        dialog.Commands.Add(
                                                            new UICommand(
                                                                "No, thanks",
                                                                (cmd) => this.settingsService.SetRoamingValue<bool>(DoNotAskToReviewKey, true)));
                                                        dialog.Commands.Add(
                                                            new UICommand(
                                                                "Remind me later",
                                                                (cmd) => this.settingsService.SetRoamingValue<int>(CountOfStartsBeforeReview, 0)));

                                                        var tResult = dialog.ShowAsync();
                                                    }
                                                    else
                                                    {
                                                        this.settingsService.SetRoamingValue<int>(CountOfStartsBeforeReview, startsCount + 1);
                                                    }
                                                }

                                                if (App.Container.Resolve<ISettingsService>().GetRoamingValue<bool>("VersionHistory v1.1"))
                                                {
                                                    this.searchService.Register();
                                                    this.navigationService.NavigateTo<IStartView>();
                                                }
                                                else
                                                {
                                                    this.navigationService.NavigateTo<IWhatIsNewView>(keepInHistory: false);
                                                }
                                            }
                                            else
                                            {
                                                this.BindingModel.Message = "Cannot load data...";
                                                this.BindingModel.IsFailed = true;
                                            }
                                        },
                                        TaskScheduler.FromCurrentSynchronizationContext());
                                }
                                else
                                {
                                    this.BindingModel.Message = "Cannot load data...";
                                    this.BindingModel.IsFailed = true;
                                }
                            },
                            TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else
                    {
                        this.BindingModel.Message = "Cannot load data...";
                        this.BindingModel.IsFailed = true;
                    }
                },
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}