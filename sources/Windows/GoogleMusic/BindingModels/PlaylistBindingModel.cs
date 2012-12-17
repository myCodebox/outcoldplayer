// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;

    public class PlaylistBindingModel : BindingModelBase
    {
        private static Random random = new Random();

        private readonly GoogleMusicPlaylist playlist;

        public PlaylistBindingModel(GoogleMusicPlaylist playlist)
        {
            this.playlist = playlist;
        }

        public string Title
        {
            get
            {
                return this.playlist.Title;
            }
        }

        public ImageSource PreviewImage
        {
            get
            {
                if (this.playlist.Playlist != null && this.playlist.Playlist.Count > 0)
                {
                    var index = random.Next(0, this.playlist.Playlist.Count - 1);
                    return new BitmapImage(new Uri("https:" + this.playlist.Playlist[index].AlbumArtUrl));
                }

                return null;
            }
        }
    }
}