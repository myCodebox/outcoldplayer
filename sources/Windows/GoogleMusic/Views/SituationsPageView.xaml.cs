// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;
    using OutcoldSolutions.GoogleMusic.BindingModels;

    public sealed partial class SituationsPageView : PageViewBase, ISituationsPageView
    {
        private IPlaylistsListView playlistsListView;

        public SituationsPageView()
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
                listView.SetBinding(
                    PlaylistsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("BindingModel.Playlists")
                    });
            }

            this.TrackScrollViewer(this);
        }
    }
}
