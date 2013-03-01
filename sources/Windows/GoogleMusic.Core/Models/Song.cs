﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    public class Song : BindingModelBase
    {
        private SongMetadata metadata;

        private bool isPlaying;

        public Song(SongMetadata metadata)
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
                return this.Metadata.Artist;
            }
        }

        public string Album
        {
            get
            {
                return this.Metadata.Album;
            }
        }

        public ushort PlayCount
        {
            get
            {
                return this.Metadata.PlayCount;
            }

            set
            {
                this.Metadata.PlayCount = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public int Rating
        {
            get
            {
                return this.Metadata.Rating;
            }

            set
            {
                this.Metadata.Rating = (byte)value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public ushort Track
        {
            get
            {
                return this.Metadata.Track;
            }
        }

        public SongMetadata Metadata
        {
            get
            {
                return this.metadata;
            }

            set
            {
                this.metadata = value;
                this.RaisePropertyChanged(() => this.Title);
                this.RaisePropertyChanged(() => this.Duration);
                this.RaisePropertyChanged(() => this.Artist);
                this.RaisePropertyChanged(() => this.Album);
                this.RaisePropertyChanged(() => this.PlayCount);
                this.RaisePropertyChanged(() => this.Rating);
            }
        }

        public bool IsPlaying
        {
            get
            {
                return this.isPlaying;
            }

            set
            {
                this.isPlaying = value;
                this.RaiseCurrentPropertyChanged();
            }
        }
    }
}