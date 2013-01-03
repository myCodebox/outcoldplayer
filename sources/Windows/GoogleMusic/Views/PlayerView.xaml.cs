// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class PlayerView : UserControl
    {
        public PlayerView()
        {
            this.InitializeComponent();

            this.MorePopup.Closed += (sender, o) => App.Container.Resolve<IMediaElemenetContainerView>().ShowAd();
        }

        private void MoreClick(object sender, RoutedEventArgs e)
        {
            this.MorePopup.IsOpen = true;
            App.Container.Resolve<IMediaElemenetContainerView>().HideAd();
        }

        private void NavigateToCurrentPlaylist(object sender, RoutedEventArgs e)
        {
            App.Container.Resolve<INavigationService>().NavigateTo<ICurrentPlaylistView>();
        }
    }
}
