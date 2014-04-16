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
    using Windows.UI.Xaml.Controls;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Input;

    using OutcoldSolutions.GoogleMusic.Presenters;

    public sealed partial class SongsListView : ViewBase, ISongsListView
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SongsListView), new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty ViewPlaylistProperty =
            DependencyProperty.Register("ViewPlaylist", typeof(object), typeof(SongsListView), new PropertyMetadata(null, OnViewPlaylistChanged));

        public static readonly DependencyProperty IsAlbumColumnVisibleProperty =
           DependencyProperty.Register("IsAlbumColumnVisible", typeof(bool), typeof(SongsListView), new PropertyMetadata(true));

        public static readonly DependencyProperty IsNumColumnVisibleProperty =
            DependencyProperty.Register("IsNumColumnVisible", typeof(bool), typeof(SongsListView), new PropertyMetadata(false));

        public SongsListView()
        {
            this.InitializeComponent();
        }

        public static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var songsListView = d as SongsListView;
            if (songsListView != null)
            {
                songsListView.GetPresenter<SongsListViewPresenter>().SetCollection(e.NewValue as IEnumerable<Song>);
            }
        }

        private static void OnViewPlaylistChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var songsListView = d as SongsListView;
            if (songsListView != null)
            {
                songsListView.GetPresenter<SongsListViewPresenter>().ViewPlaylist = e.NewValue as IPlaylist;
            }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object ViewPlaylist
        {
            get { return (object)GetValue(ViewPlaylistProperty); }
            set { SetValue(ViewPlaylistProperty, value); }
        }

        public bool IsAlbumColumnVisible
        {
            get { return (bool)GetValue(IsAlbumColumnVisibleProperty); }
            set { SetValue(IsAlbumColumnVisibleProperty, value); }
        }

        public bool IsNumColumnVisible
        {
            get { return (bool)GetValue(IsNumColumnVisibleProperty); }
            set { SetValue(IsNumColumnVisibleProperty, value); }
        }

        public int MaxItems
        {
            get { return this.GetPresenter<SongsListViewPresenter>().MaxItems; }
            set { this.GetPresenter<SongsListViewPresenter>().MaxItems = value; }
        }

        public bool AllowSorting
        {
            get { return this.GetPresenter<SongsListViewPresenter>().AllowSorting; }
            set { this.GetPresenter<SongsListViewPresenter>().AllowSorting = value; }
        }

        public ListView GetListView()
        {
            return this.ListView;
        }

        public async Task ScrollIntoCurrentSongAsync(Song song)
        {
            await Task.Run(
                async () =>
                {
                    var presenter = this.GetPresenter<SongsListViewPresenter>();
                    if (presenter.Songs != null)
                    { 
                        var songBindingModel = presenter.Songs.FirstOrDefault(x => string.Equals(x.Metadata.SongId, song.SongId));
                        if (songBindingModel != null)
                        {
                            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, this.UpdateLayout);
                            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.ListView.ScrollIntoView(songBindingModel));
                        }
                    }
                });
        }

        private void ListDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var songBindingModel = frameworkElement.DataContext as SongBindingModel;
                if (songBindingModel != null)
                {
                    this.GetPresenter<SongsListViewPresenter>().PlaySong(songBindingModel);
                }
            }
        }
    }
}
