// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml.Controls;

    public interface IPlaylistsPageView : IPageView
    {
    }

    public sealed partial class PlaylistsPageView : PageViewBase, IPlaylistsPageView
    {
        private PlaylistsPageViewPresenter presenter;

        public PlaylistsPageView()
        {
            this.InitializeComponent();
            this.TrackItemsControl(this.ListView);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.presenter = this.GetPresenter<PlaylistsPageViewPresenter>();
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            var playlistBindingModel = e.ClickedItem as PlaylistBindingModel;
            if (playlistBindingModel != null)
            {
                if (playlistBindingModel.Playlist.PlaylistType == PlaylistType.Radio)
                {
                    this.presenter.PlayPlaylist(playlistBindingModel.Playlist);
                }
                else
                {
                    this.NavigationService.NavigateToPlaylist(playlistBindingModel.Playlist);
                }
            }
        }

        private void SemanticZoom_OnViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.IsSourceZoomedInView)
            {
                var groups = this.ListViewGroups.ItemsSource as IEnumerable<PlaylistsGroupBindingModel>;
                if (groups != null)
                {
                    e.DestinationItem.Item = groups.FirstOrDefault(x => x.Playlists.Contains(e.SourceItem.Item));
                }
            }
            else
            {
                var group = e.SourceItem.Item as PlaylistsGroupBindingModel;
                if (group != null)
                {
                    e.DestinationItem.Item = group.Playlists.FirstOrDefault();
                }
            }
        }
    }
}
