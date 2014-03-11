// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    using OutcoldSolutions.GoogleMusic.Presenters;

    public interface IArtistPageView : IPageView
    {
    }

    public sealed partial class ArtistPageView : PageViewBase, IArtistPageView
    {
        private ArtistPageViewPresenter presenter;
        private IPlaylistsListView libraryAlbums;
        private IPlaylistsListView libraryCollections;
        private IPlaylistsListView allAccessAlbums;
        private IPlaylistsListView realtedArtists;
        private ISongsListView songsListView;

        public ArtistPageView()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<ArtistPageViewPresenter>();

            this.LibraryAlbumsContentPresenter.Content = (this.libraryAlbums = this.Container.Resolve<IPlaylistsListView>());
            this.LibraryCollectionsContentPresenter.Content = (this.libraryCollections = this.Container.Resolve<IPlaylistsListView>());
            this.GoogleMusicAlbumsContentPresenter.Content = (this.allAccessAlbums = this.Container.Resolve<IPlaylistsListView>());
            this.RelatedArtistsContentPresenter.Content = (this.realtedArtists = this.Container.Resolve<IPlaylistsListView>());
            this.TopSongsContentPresenter.Content = (this.songsListView = this.Container.Resolve<ISongsListView>());

            var frameworkElement = this.libraryAlbums as PlaylistsListView;
            if (frameworkElement != null)
            {
                frameworkElement.SetBinding(
                    PlaylistsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("BindingModel.Albums")
                    });
            }

            frameworkElement = this.libraryCollections as PlaylistsListView;
            if (frameworkElement != null)
            {
                frameworkElement.SetBinding(
                    PlaylistsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("BindingModel.Collections")
                    });
            }

            frameworkElement = this.allAccessAlbums as PlaylistsListView;
            if (frameworkElement != null)
            {
                frameworkElement.MaxItems = 6;
                frameworkElement.SetBinding(
                    PlaylistsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("BindingModel.GoogleMusicAlbums")
                    });
            }

            frameworkElement = this.realtedArtists as PlaylistsListView;
            if (frameworkElement != null)
            {
                frameworkElement.MaxItems = 3;
                frameworkElement.SetBinding(
                    PlaylistsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("BindingModel.RelatedArtists")
                    });
            }

            var songsListViewFrameworkElement = this.songsListView as SongsListView;
            if (songsListViewFrameworkElement != null)
            {
                songsListViewFrameworkElement.MaxItems = 5;
                songsListViewFrameworkElement.AllowSorting = false;
                songsListViewFrameworkElement.SetBinding(
                    SongsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("BindingModel.TopSongs")
                    });
            }

            this.TrackScrollViewer(this);
        }
    }
}
