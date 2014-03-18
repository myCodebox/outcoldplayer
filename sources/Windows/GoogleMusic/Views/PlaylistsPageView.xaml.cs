// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    using OutcoldSolutions.GoogleMusic.BindingModels;

    public interface IUserPlaylistsPageView : IPageView
    {
    }

    public interface IPlaylistsPageView : IPageView
    {
    }

    public interface IRadioPageView : IPageView
    {
    }

    public interface IGenrePageView : IPageView
    {
    }

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

            var frameworkElement = this.playlistsListView as PlaylistsListView;

            if (frameworkElement != null)
            {
                frameworkElement.SetBinding(
                    PlaylistsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("BindingModel.Playlists")
                    });

                this.TrackScrollViewer(frameworkElement.GetListView());
            }
        }
    }
}
