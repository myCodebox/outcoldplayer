// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;


    internal class ProgressLoadingPopupViewRequest
    {
        public ProgressLoadingPopupViewRequest(bool forceToUpdate)
        {
            this.ForceToUpdate = forceToUpdate;
        }

        public bool ForceToUpdate { get; set; }
    }

    public class ProgressLoadingPopupViewPresenter : ViewPresenterBase<IProgressLoadingPopupView>
    {
        private readonly IApplicationResources resources;
        private readonly ISettingsService settingsService;
        private readonly IInitialSynchronization initialSynchronization;

        private readonly IAnalyticsService analyticsService;
        private readonly INotificationService notificationService;
        private readonly IShellService shellService;

        internal ProgressLoadingPopupViewPresenter(
            IApplicationResources resources,
            ISettingsService settingsService, 
            IInitialSynchronization initialSynchronization,
            IAnalyticsService analyticsService,
            INotificationService notificationService,
            IShellService shellService)
        {
            this.resources = resources;
            this.settingsService = settingsService;
            this.initialSynchronization = initialSynchronization;
            this.analyticsService = analyticsService;
            this.notificationService = notificationService;
            this.shellService = shellService;
            this.BindingModel = new ProgressLoadingPageViewBindingModel();

            this.ReloadSongsCommand = new DelegateCommand(this.LoadSongs, () => this.BindingModel.IsFailed);

            this.BindingModel.Subscribe(
                () => this.BindingModel.IsFailed, (sender, args) => this.ReloadSongsCommand.RaiseCanExecuteChanged());

            this.LoadSongs();
        }

        public ProgressLoadingPageViewBindingModel BindingModel { get; private set; }

        public DelegateCommand ReloadSongsCommand { get; private set; }

        private async void LoadSongs()
        {
            this.BindingModel.IsFailed = false;

            bool isFailed = false;
            bool showErrorInfo = false;

            try
            {
                await this.InitializeRepositoriesAsync();
                this.analyticsService.SendEvent("ProgressLoading", "Loading", "Succeeded");
            }
            catch (Exception e)
            {
                var webRequestException = e as WebRequestException;
                if (webRequestException != null && webRequestException.StatusCode == HttpStatusCode.Forbidden)
                {
                    showErrorInfo = true;
                }
                else
                {
                    this.analyticsService.SendEvent("ProgressLoading", "Loading", "Failed");
                    isFailed = true;
                    this.Logger.Error(e, "Exception while tried to initialize repositories.");
                }
            }
            
            await this.Dispatcher.RunAsync(
                    async () =>
                    {
                        if (showErrorInfo)
                        {
                            try
                            {
                                await this.ShowErrorInfoAsync();
                            }
                            catch (Exception e)
                            {
                                this.Logger.Error(e, "ShowErrorInfoAsync failed");
                            }
                            
                            this.View.Close(new ProgressLoadingCloseEventArgs(isFailed: true));
                        }
                        else if (isFailed)
                        {
                            this.BindingModel.Message = this.resources.GetString("Loading_Error_Failed");
                            this.BindingModel.IsFailed = true;
                        }
                        else
                        {
                            this.View.Close(new ProgressLoadingCloseEventArgs(isFailed: false));
                        }
                    });
        }

        private async Task InitializeRepositoriesAsync()
        {
            DbContext dbContext = new DbContext();
            await dbContext.InitializeAsync();

            await this.Dispatcher.RunAsync(
                    () =>
                    {
                        this.BindingModel.Progress = 0;
                        this.BindingModel.Message = this.resources.GetString("Loading_LoadingMusicLibrary");
                    });

            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += async (sender, i) =>
            {
                await this.Dispatcher.RunAsync(
                    DispatcherPriority.High, () => { this.BindingModel.Progress = i; });
            };

            this.settingsService.ResetLibraryFreshness();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            await this.initialSynchronization.InitializeAsync(progress);

            stopwatch.Stop();

            this.analyticsService.SendTiming(stopwatch.Elapsed, "ProgressPopup", "Loading", "Library");

            await progress.SafeReportAsync(1.0);
        }

        private Task ShowErrorInfoAsync()
        {
            return this.notificationService.ShowQuestionAsync(this.resources.GetString("Loading_MessageBox_Failed.Message"),
                () => this.Logger.LogTask(this.shellService.LaunchUriAsync(new Uri("https://play.google.com/music/listen"))),
                yesButton:  this.resources.GetString("Loading_MessageBox_Failed.OkButton"),
                noButton: this.resources.GetString("Loading_MessageBox_Failed.CancelButton"));
        }
    }
}