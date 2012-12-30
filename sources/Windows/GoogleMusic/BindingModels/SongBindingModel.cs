// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;

    public class SongBindingModel : BindingModelBase
    {
        private readonly Song song;
        private bool isSelected = false;
        private int index = 0;

        public SongBindingModel(Song song)
        {
            this.song = song;
        }

        public ImageSource AlbumArt
        {
            get
            {
                if (this.song.GoogleMusicMetadata.AlbumArtUrl != null)
                {
                    // TODO: Load only 40x40 image
                    return new BitmapImage(new Uri("https:" + this.song.GoogleMusicMetadata.AlbumArtUrl));
                }

                // TODO: Some default image
                return null;
            }
        }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }

            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        public int Index
        {
            get
            {
                return this.index;
            }

            set
            {
                if (this.index != value)
                {
                    this.index = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        public Song Song
        {
            get
            {
                return this.song;
            }
        }
    }
}