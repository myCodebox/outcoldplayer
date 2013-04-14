// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml;

    public class LinksRegionViewPresenter : ViewPresenterBase<IView>
    {
        private readonly IDispatcher dispatcher;
        private readonly IGoogleMusicSynchronizationService googleMusicSynchronizationService;

        private readonly DispatcherTimer synchronizationTimer;
        private int synchronizationTime = 0; // we don't want to synchronize playlists each time, so we will do it on each 6 time

        public LinksRegionViewPresenter(
            ISearchService searchService,
            IDispatcher dispatcher,
            IGoogleMusicSynchronizationService googleMusicSynchronizationService)
        {
            this.dispatcher = dispatcher;
            this.googleMusicSynchronizationService = googleMusicSynchronizationService;
            this.ShowSearchCommand = new DelegateCommand(searchService.Activate);
            this.BindingModel = new LinksRegionBindingModel();

            this.synchronizationTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            this.synchronizationTimer.Stop();
            this.synchronizationTime = 0;

            this.synchronizationTimer.Tick += this.SynchronizationTimerOnTick;

            this.Logger.LogTask(this.Synchronize());
        }

        public DelegateCommand ShowSearchCommand { get; private set; }

        public LinksRegionBindingModel BindingModel { get; set; }

        private async void SynchronizationTimerOnTick(object sender, object o)
        {
            await this.Synchronize();
        }

        private async Task Synchronize()
        {
            await this.dispatcher.RunAsync(
                () =>
                    {
                        this.synchronizationTimer.Stop();
                        this.BindingModel.IsSynchronizing = true;
                        this.BindingModel.UpdatingText = "Updating songs...";
                    });

            bool error = false;

            try
            {
                await this.googleMusicSynchronizationService.UpdateSongsAsync();

                if (this.synchronizationTime == 0)
                {
                    await this.dispatcher.RunAsync(() => { this.BindingModel.UpdatingText = "Updating playlists..."; });
                    await this.googleMusicSynchronizationService.UpdateUserPlaylistsAsync();
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
                         this.BindingModel.IsSynchronizing = false;
                         this.BindingModel.UpdatingText = error ? "Failed to update library..." : "Library updated";
                     });
            await Task.Delay(TimeSpan.FromSeconds(2));
            await this.dispatcher.RunAsync(
                     () =>
                     {
                         this.synchronizationTime++;
                         if (this.synchronizationTime >= 6)
                         {
                             this.synchronizationTime = 0;
                         }

                         this.synchronizationTimer.Start();
                         this.BindingModel.UpdatingText = string.Empty;
                     });
        }
    }
}