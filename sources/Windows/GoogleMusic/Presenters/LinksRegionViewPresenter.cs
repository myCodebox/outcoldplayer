// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    public class LinksRegionViewPresenter : ViewPresenterBase<IView>
    {
        private readonly ISearchService searchService;

        public LinksRegionViewPresenter(
            ISearchService searchService)
        {
            this.ShowSearchCommand = new DelegateCommand(this.ShowSearch);

            this.searchService = searchService;
            this.searchService.IsRegisteredChanged +=
                (sender, args) => this.RaisePropertyChanged(() => this.IsShowSearchCommandVisible);
        }

        public DelegateCommand ShowSearchCommand { get; private set; }

        public bool IsShowSearchCommandVisible
        {
            get
            {
                return this.searchService.IsRegistered;
            }
        }

        private void ShowSearch()
        {
            this.searchService.Activate();
        }
    }
}