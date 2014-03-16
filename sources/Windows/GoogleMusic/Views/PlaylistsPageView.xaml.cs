// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;
    using System.Linq;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml.Controls;

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

            this.SemanticZoom.ZoomedInView = (this.playlistsListView = this.Container.Resolve<IPlaylistsListView>()) as PlaylistsListView;

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

        public override void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            this.SemanticZoom.IsZoomedInViewActive = true;
            base.OnNavigatedTo(eventArgs);
        }

        private void SemanticZoom_OnViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.IsSourceZoomedInView)
            {
                var groups = this.ListViewGroups.ItemsSource as IEnumerable<PlaylistsGroupBindingModel>;
                if (groups != null)
                {
                    e.DestinationItem.Item = groups.FirstOrDefault(x => x.Playlists.Any(p => p.Playlist == e.SourceItem.Item));
                }
            }
            else
            {
                var group = e.SourceItem.Item as PlaylistsGroupBindingModel;
                if (group != null)
                {
                    var groupPlaylist = group.Playlists.Select(x => x.Playlist).FirstOrDefault();
                    if (groupPlaylist != null)
                    {
                        e.DestinationItem.Item =
                            this.playlistsListView.GetPresenter<PlaylistsListViewPresenter>()
                                .Playlists.FirstOrDefault(x => x.Playlist == groupPlaylist);
                    }
                }
            }
        }
    }
}
