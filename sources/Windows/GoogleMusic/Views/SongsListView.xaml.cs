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

    public interface ISongsListView : IView
    {
        ListView GetListView();

        Task ScrollIntoCurrentSongAsync(Song song);
    }

    public sealed partial class SongsListView : ViewBase, ISongsListView
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SongsListView), new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty ViewPlaylistProperty =
            DependencyProperty.Register("ViewPlaylist", typeof(IPlaylist), typeof(SongsListView), new PropertyMetadata(null, OnViewPlaylistChanged));

        public static readonly DependencyProperty IsAlbumColumnVisibleProperty =
           DependencyProperty.Register("IsAlbumColumnVisible", typeof(bool), typeof(SongsListView), new PropertyMetadata(true));

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

        public IPlaylist ViewPlaylist
        {
            get { return (IPlaylist)GetValue(ViewPlaylistProperty); }
            set { SetValue(ViewPlaylistProperty, value); }
        }

        public bool IsAlbumColumnVisible
        {
            get { return (bool)GetValue(IsAlbumColumnVisibleProperty); }
            set { SetValue(IsAlbumColumnVisibleProperty, value); }
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
