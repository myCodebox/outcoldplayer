// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.Views;

    public class MainFramePresenter : ViewPresenterBase<IMainFrame>
    {
        private readonly INavigationService navigationService;

        private string title;

        private string subtitle;

        public MainFramePresenter(
            INavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.GoBackCommand = new DelegateCommand(this.GoBack, this.CanGoBack);
            this.navigationService.NavigatedTo += this.NavigationServiceOnNavigatedTo;
        }

        public DelegateCommand GoBackCommand { get; set; }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.SetValue(ref this.title, value);
            }
        }

        public string Subtitle
        {
            get
            {
                return this.subtitle;
            }

            set
            {
                this.SetValue(ref this.subtitle, value);
            }
        }

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