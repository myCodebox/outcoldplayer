// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Presenters;

    public class ReleasesHistoryPageViewPresenter : PagePresenterBase<IReleasesHistoryPageView>
    {
        private readonly INavigationService navigationService;

        public ReleasesHistoryPageViewPresenter(
            IDependencyResolverContainer container,
            INavigationService navigationService)
            : base(container)
        {
            this.navigationService = navigationService;

            this.LeavePageCommand = new DelegateCommand(this.LeavePage);
        }

        public DelegateCommand LeavePageCommand { get; private set; }

        private void LeavePage()
        {
            this.navigationService.NavigateTo<IStartPageView>();
        }
    }
}