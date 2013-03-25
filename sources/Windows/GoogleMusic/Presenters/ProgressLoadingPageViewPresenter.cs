// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    using Windows.System;
    using Windows.UI.Core;
    using Windows.UI.Popups;

    public class ProgressLoadingPageViewPresenter : PagePresenterBase<IPageView>
    {
        private const int AskForReviewStarts = 10;
        private const string DoNotAskToReviewKey = "DoNotAskToReviewKey";
        private const string CountOfStartsBeforeReview = "CountOfStartsBeforeReview";

        private const string CurrentVersion = "2.0.0.3";

        private readonly INavigationService navigationService;
        private readonly ISettingsService settingsService;

        private readonly IInitialSynchronization initialSynchronization;

        public ProgressLoadingPageViewPresenter(
            INavigationService navigationService,
            ISettingsService settingsService,
            IInitialSynchronization initialSynchronization)
        {
            this.navigationService = navigationService;
            this.settingsService = settingsService;
            this.initialSynchronization = initialSynchronization;
            this.BindingModel = new ProgressLoadingPageViewBindingModel();

            this.ReloadSongsCommand = new DelegateCommand(this.LoadSongs, () => this.BindingModel.IsFailed);

            this.BindingModel.Subscribe(() => this.BindingModel.IsFailed, (sender, args) => this.ReloadSongsCommand.RaiseCanExecuteChanged());
        }

        public ProgressLoadingPageViewBindingModel BindingModel { get; private set; }

        public DelegateCommand ReloadSongsCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);

            this.LoadSongs();
        }

        protected override Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            return Task.Delay(0);
        }

        public async void LoadSongs()
        {
            this.BindingModel.IsFailed = false;

            var currentVersion = this.settingsService.GetValue<string>("Version", null);
            bool fCurrentVersion = string.Equals(currentVersion, CurrentVersion, StringComparison.OrdinalIgnoreCase);
            bool fUpdate = !fCurrentVersion && currentVersion != null;

            try
            {
                await this.InitializeRepositoriesAsync();
            }
            catch (Exception e)
            {
                this.Logger.LogErrorException(e);

                this.BindingModel.Message = "Cannot load data. Verify your network connection.";
                this.BindingModel.IsFailed = true;

                return;
            }
            
            this.VerifyIfCanAskForReview();

            if (!fUpdate)
            {
                this.navigationService.NavigateTo<IStartPageView>();
            }
            else
            {
                this.settingsService.SetValue("Version", CurrentVersion);
                this.navigationService.NavigateTo<IReleasesHistoryPageView>(keepInHistory: false);
            }
        }

        private async Task InitializeRepositoriesAsync()
        {
            DbContext dbContext = new DbContext();
            await dbContext.InitializeAsync();

            await this.Dispatcher.RunAsync(
                () =>
                {
                    this.BindingModel.Progress = 0;
                    this.BindingModel.Message = "Loading music library...";
                });

            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += async (sender, i) =>
            {
                await this.Dispatcher.RunAsync(
                    CoreDispatcherPriority.High,
                    () =>
                    {
                        this.BindingModel.Progress = i;
                    });
            };

            if (!this.settingsService.GetLibraryFreshnessDate().HasValue)
            {
                await this.initialSynchronization.InitializeAsync(progress);
            }

            await progress.SafeReportAsync(1.0);
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