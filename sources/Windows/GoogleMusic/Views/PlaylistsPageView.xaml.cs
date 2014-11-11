// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Data;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;

    public sealed partial class PlaylistsPageView : PageViewBase, IPlaylistsPageView, IUserPlaylistsPageView, IRadioPageView, IGenrePageView
    {
        private IPlaylistsListView playlistsListView;

        public PlaylistsPageView()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var presenter = this.GetPresenter<BindingModelBase>();

            this.PlaylistsContentPresenter.Content = (this.playlistsListView = this.Container.Resolve<IPlaylistsListView>()) as PlaylistsListView;

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

                this.TrackScrollViewer((ListView)listView.GetListView());
            }
        }
    }
}
