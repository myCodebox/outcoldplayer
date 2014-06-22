// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;

    public class SearchPageViewBindingModel : BindingModelBase
    {
        private string searchText;
        private bool isLocalOnly;
        private bool isOnline;

        private IList<IPlaylist> artists;
        private IList<IPlaylist> albums;
        private IList<IPlaylist> genres;
        private IList<IPlaylist> radioStations;
        private IList<IPlaylist> userPlaylists;
        private IList<Song> songs;

        public string SearchText
        {
            get
            {
                return this.searchText;
            }

            set
            {
                this.SetValue(ref this.searchText, value);
            }
        }

        public bool IsOnline
        {
            get
            {
                return this.isOnline;
            }

            set
            {
                this.SetValue(ref this.isOnline, value);
            }
        }

        public IList<IPlaylist> Artists
        {
            get
            {
                return this.artists;
            }
            set
            {
                this.SetValue(ref this.artists, value);
            }
        }

        public IList<IPlaylist> Albums
        {
            get
            {
                return this.albums;
            }
            set
            {
                this.SetValue(ref this.albums, value);
            }
        }

        public IList<IPlaylist> Genres
        {
            get
            {
                return this.genres;
            }
            set
            {
                this.SetValue(ref this.genres, value);
            }
        }

        public IList<IPlaylist> RadioStations
        {
            get
            {
                return this.radioStations;
            }
            set
            {
                this.SetValue(ref this.radioStations, value);
            }
        }

        public IList<IPlaylist> UserPlaylists
        {
            get
            {
                return this.userPlaylists;
            }
            set
            {
                this.SetValue(ref this.userPlaylists, value);
            }
        }

        public IList<Song> Songs
        {
            get
            {
                return this.songs;
            }
            set
            {
                this.SetValue(ref this.songs, value);
            }
        }
    }
}