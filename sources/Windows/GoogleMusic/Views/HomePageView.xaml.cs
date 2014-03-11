// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    using OutcoldSolutions.GoogleMusic.BindingModels;

    public interface IHomePageView : IPageView
    {
    }

    public sealed partial class HomePageView : PageViewBase, IHomePageView
    {
        private IPlaylistsListView playlistsListView;

        public HomePageView()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var presenter = this.GetPresenter<BindingModelBase>();

            this.PlaylistsContentPresenter.Content = this.playlistsListView = this.Container.Resolve<IPlaylistsListView>();

            ((PlaylistsListView)this.playlistsListView).IsMixedList = true;
            ((PlaylistsListView)this.playlistsListView).SetBinding(
                    PlaylistsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("BindingModel.Playlists")
                    });

            this.TrackScrollViewer(this.playlistsListView.GetListView());
        }
    }
}
