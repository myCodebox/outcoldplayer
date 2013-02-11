// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;

    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.System;
    using Windows.UI.Xaml;

    public sealed partial class ProgressLoadingPageView : PageViewBase, IProgressLoadingView
    {
        private readonly ProgressLoadingPresenter presenter;

        public ProgressLoadingPageView()
        {
            this.InitializeComponent();
            this.presenter = this.InitializePresenter<ProgressLoadingPresenter>();
        }

        private void TryAgainClick(object sender, RoutedEventArgs e)
        {
            if (this.presenter.BindingModel.IsFailed)
            {
                this.presenter.LoadSongs();
            }
        }

        private void TwitterFollowClick(object sender, RoutedEventArgs e)
        {
            var result = Launcher.LaunchUriAsync(new Uri("https://twitter.com/gMusicW"));
        }

        private void UserVoiceClick(object sender, RoutedEventArgs e)
        {
            var result = Launcher.LaunchUriAsync(new Uri("https://gmusic.uservoice.com"));
        }
    }
}
