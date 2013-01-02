// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml;

    public sealed partial class ProgressLoadingView : ViewBase, IProgressLoadingView
    {
        public ProgressLoadingView()
        {
            this.InitializeComponent();
            this.InitializePresenter<ProgressLoadingPresenter>();
        }

        private void TryAgainClick(object sender, RoutedEventArgs e)
        {
            if (this.Presenter<ProgressLoadingPresenter>().BindingModel.IsFailed)
            {
                this.Presenter<ProgressLoadingPresenter>().LoadSongs();
            }
        }
    }
}
