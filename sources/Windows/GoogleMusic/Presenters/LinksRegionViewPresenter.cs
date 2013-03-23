// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml;

    public class LinksRegionViewPresenter : ViewPresenterBase<IView>
    {
        private DispatcherTimer synchronizationTimer;
        private int synchronizationTime; // we don't want to synchronize playlists each time, so we will do it on each 6 time

        public LinksRegionViewPresenter(
            ISearchService searchService)
        {
            this.ShowSearchCommand = new DelegateCommand(searchService.Activate);
            this.BindingModel = new LinksRegionBindingModel();

            this.synchronizationTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            this.synchronizationTime = 0;
        }

        public DelegateCommand ShowSearchCommand { get; private set; }

        public LinksRegionBindingModel BindingModel { get; set; }
    }
}