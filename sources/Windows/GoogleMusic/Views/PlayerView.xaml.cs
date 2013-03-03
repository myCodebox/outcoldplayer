// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml;

    public interface IPlayerView : IView
    {
    }

    public sealed partial class PlayerView : ViewBase, IPlayerView
    {
        public PlayerView()
        {
            this.InitializeComponent();
        }

        private void MoreClick(object sender, RoutedEventArgs e)
        {
            this.MorePopup.IsOpen = true;
        }

        private void NavigateToCurrentPlaylist(object sender, RoutedEventArgs e)
        {
            ApplicationBase.Container.Resolve<INavigationService>().NavigateTo<ICurrentPlaylistPageView>().SelectPlayingSong();
        }
    }
}
