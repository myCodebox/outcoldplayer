// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class SongBindingModel : BindingModelBase
    {
        private Song metadata;
        private SongState songState = SongState.None;

        public SongBindingModel(Song metadata)
        {
            this.metadata = metadata;
        }

        public string Title
        {
            get
            {
                return this.Metadata.Title;
            }
        }

        public double Duration
        {
            get
            {
                return this.Metadata.Duration.TotalSeconds;
            }
        }

        public string Artist
        {
            get
            {
                return string.IsNullOrEmpty(this.Metadata.ArtistTitle) ? this.Metadata.AlbumArtistTitle : this.Metadata.ArtistTitle;
            }
        }

        public string Album
        {
            get
            {
                return this.Metadata.AlbumTitle;
            }
        }

        public uint PlayCount
        {
            get
            {
                return this.Metadata.PlayCount;
            }
        }

        public int Rating
        {
            get
            {
                return this.Metadata.Rating;
            }
        }

        public ushort? Track
        {
            get
            {
                return this.Metadata.Track;
            }
        }

        public bool IsCached
        {
            get
            {
                return this.Metadata.IsCached;
            }

            set
            {
                this.Metadata.IsCached = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public Song Metadata
        {
            get
            {
                return this.metadata;
            }

            set
            {
                this.metadata = value;
                this.RaiseAllPropertiesChanged();
            }
        }

        public SongState State
        {
            get
            {
                return this.songState;
            }

            set
            {
                this.songState = value;
                this.RaiseCurrentPropertyChanged();
                this.RaisePropertyChanged(() => this.IsPlaying);
            }
        }

        public bool IsPlaying
        {
            get
            {
                return this.songState == SongState.Playing || this.songState == SongState.Paused;
            }
        }

        public void RaiseAllPropertiesChanged()
        {
            this.RaisePropertyChanged(() => this.Title);
            this.RaisePropertyChanged(() => this.Duration);
            this.RaisePropertyChanged(() => this.Artist);
            this.RaisePropertyChanged(() => this.Album);
            this.RaisePropertyChanged(() => this.PlayCount);
            this.RaisePropertyChanged(() => this.Rating);
        }
    }
}