// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml.Controls;

    public interface IUserPlaylistsPageView : IPageView
    {
    }

    public sealed partial class UserPlaylistsPageView : PageViewBase, IUserPlaylistsPageView
    {
        private UserPlaylistsPageViewPresenter presenter;

        public UserPlaylistsPageView()
        {
            this.InitializeComponent();
            this.TrackItemsControl(this.ListView);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.presenter = this.GetPresenter<UserPlaylistsPageViewPresenter>();
            this.presenter.BindingModel.SelectedItems.CollectionChanged += this.SelectedItemsOnCollectionChanged;
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

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            CollectionExtensions.UpdateCollection(this.ListView.SelectedItems, notifyCollectionChangedEventArgs.NewItems, notifyCollectionChangedEventArgs.OldItems);
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionExtensions.UpdateCollection(this.presenter.BindingModel.SelectedItems, e.AddedItems, e.RemovedItems);
        }
    }
}
