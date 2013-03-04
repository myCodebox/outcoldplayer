// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;

    using OutcoldSolutions.GoogleMusic.Services.Shell;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    public class LinksRegionViewPresenter : ViewPresenterBase<IView>
    {
        private readonly ISearchService searchService;

        public LinksRegionViewPresenter(
            IDependencyResolverContainer container,
            ISearchService searchService)
            : base(container)
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