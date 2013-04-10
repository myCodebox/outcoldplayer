// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;
    using OutcoldSolutions.Presenters;

    using Windows.UI.Core;

    public class ProgressLoadingPopupViewPresenter : ViewPresenterBase<IProgressLoadingPopupView>
    {
        private readonly ISettingsService settingsService;

        private readonly IInitialSynchronization initialSynchronization;

        public ProgressLoadingPopupViewPresenter(
            ISettingsService settingsService, IInitialSynchronization initialSynchronization)
        {
            this.settingsService = settingsService;
            this.initialSynchronization = initialSynchronization;
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

            try
            {
                await this.InitializeRepositoriesAsync();
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Exception while tried to initialize repositories.");

                this.BindingModel.Message = "Cannot load data. Verify your network connection.";
                this.BindingModel.IsFailed = true;
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
                        CoreDispatcherPriority.High, () => { this.BindingModel.Progress = i; });
                };

            this.settingsService.ResetLibraryFreshness();
            await this.initialSynchronization.InitializeAsync(progress);

            await progress.SafeReportAsync(1.0);

            this.View.Close();
        }
    }
}