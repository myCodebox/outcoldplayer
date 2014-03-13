// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;

    public interface IPlaylistsListView : IView
    {
        ListView GetListView();

        Task ScrollIntoCurrentSongAsync(IPlaylist song);
    }

    public sealed partial class PlaylistsListView : ViewBase, IPlaylistsListView, ISemanticZoomInformation
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(PlaylistsListView), new PropertyMetadata(null, OnItemsSourceChanged));

        public PlaylistsListView()
        {
            this.InitializeComponent();
        }

        public int MaxItems
        {
            get { return this.GetPresenter<PlaylistsListViewPresenter>().MaxItems; }
            set { this.GetPresenter<PlaylistsListViewPresenter>().MaxItems = value; }
        }

        public bool IsMixedList
        {
            get { return this.GetPresenter<PlaylistsListViewPresenter>().IsMixedList; }
            set { this.GetPresenter<PlaylistsListViewPresenter>().IsMixedList = value; }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var songsListView = d as PlaylistsListView;
            if (songsListView != null)
            {
                songsListView.GetPresenter<PlaylistsListViewPresenter>().SetCollection(e.NewValue as IEnumerable<IPlaylist>);
            }
        }

        public ListView GetListView()
        {
            return this.ListView;
        }

        public async Task ScrollIntoCurrentSongAsync(IPlaylist playlist)
        {
            await Task.Run(
                async () =>
                {
                    var presenter = this.GetPresenter<PlaylistsListViewPresenter>();
                    if (presenter.Playlists != null)
                    {
                        var bindingModel = presenter.Playlists.FirstOrDefault(x => string.Equals(x.Playlist.Id, playlist.Id));
                        if (bindingModel != null)
                        {
                            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, this.UpdateLayout);
                            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.ListView.ScrollIntoView(bindingModel));
                        }
                    }
                });
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            var playlistBindingModel = e.ClickedItem as PlaylistBindingModel;
            if (playlistBindingModel != null)
            {
                this.Container.Resolve<INavigationService>().NavigateToPlaylist(playlistBindingModel.Playlist);
            }
        }

        public void InitializeViewChange()
        {
            this.ListView.InitializeViewChange();
        }

        public void CompleteViewChange()
        {
            this.ListView.CompleteViewChange();
        }

        public void MakeVisible(SemanticZoomLocation item)
        {
            this.ListView.MakeVisible(item);
        }

        public void StartViewChangeFrom(SemanticZoomLocation source, SemanticZoomLocation destination)
        {
            this.ListView.StartViewChangeFrom(source, destination);
        }

        public void StartViewChangeTo(SemanticZoomLocation source, SemanticZoomLocation destination)
        {
            this.ListView.StartViewChangeTo(source, destination);
        }

        public void CompleteViewChangeFrom(SemanticZoomLocation source, SemanticZoomLocation destination)
        {
            this.ListView.CompleteViewChangeFrom(source, destination);
        }

        public void CompleteViewChangeTo(SemanticZoomLocation source, SemanticZoomLocation destination)
        {
            this.ListView.CompleteViewChangeTo(source, destination);
        }

        public bool IsActiveView
        {
            get
            {
                return this.ListView.IsActiveView;
            }
            set
            {
                this.ListView.IsActiveView = value;
            }
        }

        public bool IsZoomedInView
        {
            get
            {
                return this.ListView.IsZoomedInView;
            }
            set
            {
                this.ListView.IsZoomedInView = value;
            }
        }

        public SemanticZoom SemanticZoomOwner
        {
            get
            {
                return this.ListView.SemanticZoomOwner;
            }
            set
            {
                this.ListView.SemanticZoomOwner = value;
            }
        }
    }
}
