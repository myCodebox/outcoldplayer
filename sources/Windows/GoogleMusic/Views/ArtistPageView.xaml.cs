// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml.Controls;

    public interface IArtistPageView : IPageView
    {
    }

    public sealed partial class ArtistPageView : PageViewBase, IArtistPageView
    {
        private ArtistPageViewPresenter presenter;

        public ArtistPageView()
        {
            this.InitializeComponent();
            this.TrackItemsControl(this.ListView);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.presenter = this.GetPresenter<ArtistPageViewPresenter>();
            this.presenter.BindingModel.SelectedItems.CollectionChanged += this.SelectedItemsOnCollectionChanged;
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as PlaylistBindingModel;

            Debug.Assert(album != null, "album != null");
            if (album != null)
            {
                this.NavigationService.NavigateToPlaylist(album.Playlist);
            }
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionExtensions.UpdateCollection(this.ListView.SelectedItems, e.NewItems, e.OldItems);
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionExtensions.UpdateCollection(this.presenter.BindingModel.SelectedItems, e.AddedItems, e.RemovedItems);
        }
    }
}
