// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class Song : BindingModelBase
    {
        private GoogleMusicSong googleMusicSong;

        private bool isPlaying;

        public Song(GoogleMusicSong googleMusicSong)
        {
            this.googleMusicSong = googleMusicSong;
        }

        public string Title
        {
            get
            {
                return this.GoogleMusicMetadata.Title;
            }
        }

        public double Duration
        {
            get
            {
                return TimeSpan.FromMilliseconds(this.GoogleMusicMetadata.DurationMillis).TotalSeconds;
            }
        }

        public string Artist
        {
            get
            {
                return this.GoogleMusicMetadata.Artist;
            }
        }

        public string Album
        {
            get
            {
                return this.GoogleMusicMetadata.Album;
            }
        }

        public int PlayCount
        {
            get
            {
                return this.GoogleMusicMetadata.PlayCount;
            }

            set
            {
                this.GoogleMusicMetadata.PlayCount = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public int Rating
        {
            get
            {
                return this.GoogleMusicMetadata.Rating;
            }

            set
            {
                this.GoogleMusicMetadata.Rating = value;
            }
        }

        public GoogleMusicSong GoogleMusicMetadata
        {
            get
            {
                return this.googleMusicSong;
            }

            set
            {
                this.googleMusicSong = value;
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