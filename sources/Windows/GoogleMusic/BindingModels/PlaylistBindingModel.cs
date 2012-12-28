// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;

    public class PlaylistBindingModel : BindingModelBase
    {
        private static readonly Random Random = new Random();

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

        public int SongsCount
        {
            get
            {
                if (this.playlist.Playlist == null)
                {
                    return 0;
                }

                return this.playlist.Playlist.Count;
            }
        }

        public double Duration
        {
            get
            {
                if (this.playlist.Playlist == null)
                {
                    return TimeSpan.Zero.TotalSeconds;
                }

                return TimeSpan.FromMilliseconds(this.playlist.Playlist.Sum(x => x.DurationMillis)).TotalSeconds;
            }
        }

        public ImageSource PreviewImage
        {
            get
            {
                if (this.playlist.Playlist != null && this.playlist.Playlist.Count > 0)
                {
                    var songsWithArt = this.playlist.Playlist.Where(x => x.AlbumArtUrl != null).ToList();

                    if (songsWithArt.Count > 0)
                    {
                        var index = Random.Next(0, songsWithArt.Count - 1);
                        if (this.playlist.Playlist[index].AlbumArtUrl != null)
                        {
                            // TODO: Load only 40x40 image
                            return new BitmapImage(new Uri("https:" + songsWithArt[index].AlbumArtUrl));
                        }
                    }
                }

                return null;
            }
        }

        public GoogleMusicPlaylist GetPlaylist()
        {
            return this.playlist;
        }
    }
}