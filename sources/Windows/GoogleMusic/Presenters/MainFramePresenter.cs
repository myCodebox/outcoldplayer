// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.Views;

    internal class MainFramePresenter : ViewPresenterBase<IMainFrame>
    {
        private readonly INavigationService navigationService;

        public MainFramePresenter(
            INavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.GoBackCommand = new DelegateCommand(this.GoBack, this.CanGoBack);
            this.navigationService.NavigatedTo += this.NavigationServiceOnNavigatedTo;
        }

        public DelegateCommand GoBackCommand { get; set; }

        public bool IsBackButtonVisible
        {
            get
            {
                return this.navigationService.CanGoBack();
            }
        }

        private bool CanGoBack()
        {
            return this.navigationService.CanGoBack();
        }

        private void GoBack()
        {
            if (this.CanGoBack())
            {
                this.navigationService.GoBack();
            }
        }

        private void NavigationServiceOnNavigatedTo(object sender, NavigatedToEventArgs navigatedToEventArgs)
        {
            this.GoBackCommand.RaiseCanExecuteChanged();
            this.RaisePropertyChanged(() => this.IsBackButtonVisible);
        }
    }
}