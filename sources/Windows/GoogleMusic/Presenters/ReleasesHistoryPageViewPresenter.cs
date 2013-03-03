// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Presenters;

    public class ReleasesHistoryPageViewPresenter : PagePresenterBase<IReleasesHistoryPageView>
    {
        private readonly INavigationService navigationService;
        private readonly ISearchService searchService;

        public ReleasesHistoryPageViewPresenter(
            IDependencyResolverContainer container,
            INavigationService navigationService,
            ISearchService searchService)
            : base(container)
        {
            this.navigationService = navigationService;
            this.searchService = searchService;

            this.LeavePageCommand = new DelegateCommand(this.LeavePage);
        }

        public DelegateCommand LeavePageCommand { get; private set; }

        private void LeavePage()
        {
            this.navigationService.NavigateTo<IStartPageView>();
            this.searchService.Register();
        }
    }
}