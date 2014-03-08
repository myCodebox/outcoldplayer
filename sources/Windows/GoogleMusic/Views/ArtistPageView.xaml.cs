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
        }
    }
}
