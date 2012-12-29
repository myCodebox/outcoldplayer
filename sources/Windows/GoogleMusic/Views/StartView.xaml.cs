//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public interface IStartView : IView
    {
    }

    public sealed partial class StartView : ViewBase, IStartView
    {
        public StartView()
        {
            this.InitializePresenter<StartViewPresenter>();
            this.InitializeComponent();
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            this.Presenter<StartViewPresenter>().ItemClick(e.ClickedItem as PlaylistBindingModel);
        }

        private void StartPlaylistClick(object sender, RoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var playlistBindingModel = frameworkElement.DataContext as PlaylistBindingModel;
                if (playlistBindingModel != null)
                {
                    this.Presenter<StartViewPresenter>().StartPlaylist(playlistBindingModel);
                }
            }
        }

        private void NavigateToPlaylists(object sender, RoutedEventArgs e)
        {
            App.Container.Resolve<INavigationService>().NavigateTo<IPlaylistsView>(PlaylistsRequest.Playlists);
        }

        private void NavigateToAlbums(object sender, RoutedEventArgs e)
        {
            App.Container.Resolve<INavigationService>().NavigateTo<IPlaylistsView>(PlaylistsRequest.Albums);
        }
    }
}
