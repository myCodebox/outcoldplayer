// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    using OutcoldSolutions.GoogleMusic.Presenters;

    public interface ISearchPageView : IPageView
    {
    }

    public sealed partial class SearchPageView : PageViewBase, ISearchPageView
    {
        private IPlaylistsListView albums;
        private IPlaylistsListView artists;
        private IPlaylistsListView genres;
        private IPlaylistsListView userPlaylists;
        private IPlaylistsListView radioStations;
        private ISongsListView songs;

        public SearchPageView()
        {
            this.InitializeComponent();
            
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var presenter = this.GetPresenter<SearchPageViewPresenter>();

            this.AlbumsContentPresenter.Content = (this.albums = this.Container.Resolve<IPlaylistsListView>());
            this.ArtistsContentPresenter.Content = (this.artists = this.Container.Resolve<IPlaylistsListView>());
            this.GenresContentPresenter.Content = (this.genres = this.Container.Resolve<IPlaylistsListView>());
            this.RadioStationsContentPresenter.Content = (this.radioStations = this.Container.Resolve<IPlaylistsListView>());
            this.UserPlaylistsContentPresenter.Content = (this.userPlaylists = this.Container.Resolve<IPlaylistsListView>());
            this.SongsContentPresenter.Content = (this.songs = this.Container.Resolve<ISongsListView>());

            ((PlaylistsListView)this.albums).MaxItems = 5;
            ((PlaylistsListView)this.albums).SetBinding(
                PlaylistsListView.ItemsSourceProperty,
                new Binding()
                {
                    Source = presenter,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath("BindingModel.Albums")
                });

            ((PlaylistsListView)this.artists).MaxItems = 5;
            ((PlaylistsListView)this.artists).SetBinding(
                PlaylistsListView.ItemsSourceProperty,
                new Binding()
                {
                    Source = presenter,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath("BindingModel.Artists")
                });

            ((PlaylistsListView)this.genres).MaxItems = 5;
            ((PlaylistsListView)this.genres).SetBinding(
                PlaylistsListView.ItemsSourceProperty,
                new Binding()
                {
                    Source = presenter,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath("BindingModel.Genres")
                });

            ((PlaylistsListView)this.userPlaylists).MaxItems = 5;
            ((PlaylistsListView)this.userPlaylists).SetBinding(
                PlaylistsListView.ItemsSourceProperty,
                new Binding()
                {
                    Source = presenter,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath("BindingModel.UserPlaylists")
                });

            ((PlaylistsListView)this.radioStations).MaxItems = 5;
            ((PlaylistsListView)this.radioStations).SetBinding(
                PlaylistsListView.ItemsSourceProperty,
                new Binding()
                {
                    Source = presenter,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath("BindingModel.RadioStations")
                });

            ((SongsListView)this.songs).AllowSorting = false;
            ((SongsListView)this.songs).MaxItems = 5;
            ((SongsListView)this.songs).SetBinding(
                SongsListView.ItemsSourceProperty,
                new Binding()
                {
                    Source = presenter,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath("BindingModel.Songs")
                });

            this.TrackScrollViewer(this.ScrollViewer);
        }

        public override void OnDataLoaded(NavigatedToEventArgs eventArgs)
        {
            base.OnDataLoaded(eventArgs);
            this.SearchTextBox.Focus(FocusState.Keyboard);
        }
    }
}
