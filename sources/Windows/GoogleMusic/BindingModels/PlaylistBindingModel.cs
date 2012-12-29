// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;

    public class PlaylistBindingModel : BindingModelBase
    {
        private readonly Playlist playlist;

        public PlaylistBindingModel(Playlist playlist)
        {
            this.playlist = playlist;
            this.PlayCommand = new DelegateCommand(() =>
                {
                    var currentPlaylistService = App.Container.Resolve<ICurrentPlaylistService>();

                    currentPlaylistService.ClearPlaylist();
                    if (playlist.Songs.Count > 0)
                    {
                        currentPlaylistService.AddSongs(playlist.Songs);
                        currentPlaylistService.PlayAsync(0);
                    }

                    App.Container.Resolve<INavigationService>().NavigateTo<IPlaylistView>(playlist);
                });
        }

        public DelegateCommand PlayCommand { get; private set; }

        public bool IsAlbum
        {
            get
            {
                return this.playlist is Album;
            }
        }

        public ImageSource PreviewImage
        {
            get
            {
                if (!string.IsNullOrEmpty(this.playlist.AlbumArtUrl))
                {
                    // TODO: Load only 40x40 image
                    return new BitmapImage(new Uri("https:" + this.playlist.AlbumArtUrl));
                }

                return null;
            }
        }

        public Playlist Playlist
        {
            get
            {
                return this.playlist;
            }
        }
    }
}