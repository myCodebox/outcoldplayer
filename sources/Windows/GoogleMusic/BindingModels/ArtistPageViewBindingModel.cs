// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;

    public class ArtistPageViewBindingModel : BindingModelBase
    {
        private Artist artist;
        private IList<IPlaylist> albums;
        private IList<IPlaylist> collections;
        private IList<Song> topSongs;
        private IList<IPlaylist> googleMusicAlbums;
        private IList<IPlaylist> realatedArtists;

        public ArtistPageViewBindingModel()
        {
        }

        public Artist Artist
        {
            get
            {
                return this.artist;
            }

            set
            {
                this.SetValue(ref this.artist, value);
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

        public IList<IPlaylist> Collections
        {
            get
            {
                return this.collections;
            }

            set
            {
                this.SetValue(ref this.collections, value);
            }
        }

        public IList<Song> TopSongs
        {
            get
            {
                return this.topSongs;
            }

            set
            {
                this.SetValue(ref this.topSongs, value);
            }
        }

        public IList<IPlaylist> GoogleMusicAlbums
        {
            get
            {
                return this.googleMusicAlbums;
            }

            set
            {
                this.SetValue(ref this.googleMusicAlbums, value);
            }
        }

        public IList<IPlaylist> RelatedArtists
        {
            get
            {
                return this.realatedArtists;
            }

            set
            {
                this.SetValue(ref this.realatedArtists, value);
            }
        }
    }
}