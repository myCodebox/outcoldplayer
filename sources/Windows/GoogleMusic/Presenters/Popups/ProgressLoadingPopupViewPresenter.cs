// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;
    using OutcoldSolutions.Presenters;

    using Windows.System;
    using Windows.UI.Core;
    using Windows.UI.Popups;

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
        private readonly ProgressLoadingPopupViewRequest request;

        internal ProgressLoadingPopupViewPresenter(
            IApplicationResources resources,
            ISettingsService settingsService, 
            IInitialSynchronization initialSynchronization,
            ProgressLoadingPopupViewRequest request)
        {
            this.resources = resources;
            this.settingsService = settingsService;
            this.initialSynchronization = initialSynchronization;
            this.request = request;
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
                    isFailed = true;
                    this.Logger.Error(e, "Exception while tried to initialize repositories.");
                }
            }
            
            await this.Dispatcher.RunAsync(
                    async () =>
                    {
                        if (showErrorInfo)
                        {
                            await this.ShowErrorInfoAsync();
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
            var updateInformation = await dbContext.InitializeAsync(this.request.ForceToUpdate);

            if (updateInformation.Status == DbContext.DatabaseStatus.New)
            {
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
                            CoreDispatcherPriority.High, () => { this.BindingModel.Progress = i; });
                    };

                this.settingsService.ResetLibraryFreshness();

                await this.initialSynchronization.InitializeAsync(progress);

                await progress.SafeReportAsync(1.0);
            }
        }

        private Task ShowErrorInfoAsync()
        {
            var dialog = new MessageDialog(this.resources.GetString("Loading_MessageBox_Failed.Message"));
            dialog.Commands.Add(
                new UICommand(
                    this.resources.GetString("Loading_MessageBox_Failed.OkButton"),
                    (cmd) => this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri("https://play.google.com/music/listen")).AsTask())));

            dialog.Commands.Add(new UICommand(this.resources.GetString("Loading_MessageBox_Failed.CancelButton")));
            
            return dialog.ShowAsync().AsTask();
        }
    }
}