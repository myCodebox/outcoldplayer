// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml.Controls;

    public interface IPlaylistsPageView : IPageView
    {
    }

    public sealed partial class PlaylistsPageView : PageViewBase, IPlaylistsPageView
    {
        public PlaylistsPageView()
        {
            this.InitializeComponent();
            this.TrackItemsControl(this.ListView);
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            var playlistBindingModel = e.ClickedItem as PlaylistBindingModel;
            if (playlistBindingModel != null)
            {
                this.NavigationService.NavigateToPlaylist(playlistBindingModel.Playlist);
            }
        }
        
        private void SemanticZoom_OnViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.IsSourceZoomedInView)
            {
                e.DestinationItem.Item = ((List<PlaylistsGroupBindingModel>)this.ListViewGroups.ItemsSource)
                    .FirstOrDefault(x => x.Playlists.Contains(e.SourceItem.Item));
            }
            else
            {
                e.DestinationItem.Item = ((PlaylistsGroupBindingModel)e.SourceItem.Item).Playlists.FirstOrDefault();
            }
        }
    }
}
