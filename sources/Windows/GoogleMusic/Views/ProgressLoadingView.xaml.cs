// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml;

    public sealed partial class ProgressLoadingPageView : PageViewBase, IProgressLoadingView
    {
        private ProgressLoadingPresenter presenter;

        public ProgressLoadingPageView()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<ProgressLoadingPresenter>();
        }

        private void TryAgainClick(object sender, RoutedEventArgs e)
        {
            if (this.presenter.BindingModel.IsFailed)
            {
                this.presenter.LoadSongs();
            }
        }
    }
}
