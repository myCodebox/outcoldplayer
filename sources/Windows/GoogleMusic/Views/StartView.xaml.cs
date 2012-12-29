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

        private void NavigateToPlaylists(object sender, RoutedEventArgs e)
        {
            App.Container.Resolve<INavigationService>().NavigateTo<IPlaylistsView>(PlaylistsRequest.Playlists);
        }

        private void NavigateToAlbums(object sender, RoutedEventArgs e)
        {
            App.Container.Resolve<INavigationService>().NavigateTo<IPlaylistsView>(PlaylistsRequest.Albums);
        }

        private void NavigateToGenres(object sender, RoutedEventArgs e)
        {
            App.Container.Resolve<INavigationService>().NavigateTo<IPlaylistsView>(PlaylistsRequest.Genres);
        }

        private void NavigateToArtists(object sender, RoutedEventArgs e)
        {
            App.Container.Resolve<INavigationService>().NavigateTo<IPlaylistsView>(PlaylistsRequest.Artists);
        }
    }
}
