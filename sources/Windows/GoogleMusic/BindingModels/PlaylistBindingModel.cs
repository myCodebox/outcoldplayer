// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;

    public class PlaylistBindingModel : BindingModelBase
    {
        private readonly Playlist playlist;

        public PlaylistBindingModel(Playlist playlist)
        {
            this.playlist = playlist;
        }

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