// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;

    public sealed partial class HomePageView : PageViewBase, IHomePageView
    {
        private IPlaylistsListView systemPlaylistsListView;
        private IPlaylistsListView playlistsListView;
        private IPlaylistsListView situationsListView;

        public HomePageView()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var presenter = this.GetPresenter<BindingModelBase>();

            this.PlaylistsContentPresenter.Content = (this.playlistsListView = this.Container.Resolve<IPlaylistsListView>()) as PlaylistsListView;
            this.SystemPlaylistsContentPresenter.Content = (this.systemPlaylistsListView = this.Container.Resolve<IPlaylistsListView>()) as PlaylistsListView;
            this.SituationsContentPresenter.Content = (this.situationsListView = this.Container.Resolve<IPlaylistsListView>()) as PlaylistsListView;

            var listView = this.playlistsListView as PlaylistsListView;
            if (listView != null)
            {
                listView.IsMixedList = ((IPlaylistsPageViewPresenterBase)presenter).IsMixedList;
                listView.SetBinding(
                    PlaylistsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("BindingModel.Playlists")
                    });
            }

            listView = this.systemPlaylistsListView as PlaylistsListView;
            if (listView != null)
            {
                listView.IsMixedList = ((IPlaylistsPageViewPresenterBase)presenter).IsMixedList;
                listView.SetBinding(
                    PlaylistsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("BindingModel.SystemPlaylists")
                    });
            }

            listView = this.situationsListView as PlaylistsListView;
            if (listView != null)
            {
                listView.IsMixedList = ((IPlaylistsPageViewPresenterBase)presenter).IsMixedList;
                listView.SetBinding(
                    PlaylistsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("BindingModel.Situations")
                    });
            }

            this.TrackScrollViewer(this);
        }
    }
}
