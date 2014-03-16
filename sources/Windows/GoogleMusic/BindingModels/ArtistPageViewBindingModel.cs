// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.Models;

    public class ArtistPageViewBindingModel : BindingModelBase
    {
        private Artist artist;
        private ArtistInfo artistInfo;
        private IList<IPlaylist> albums;
        private IList<IPlaylist> collections;

        private bool isAllAccessLoading;

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

        public ArtistInfo ArtistInfo
        {
            get
            {
                return this.artistInfo;
            }

            set
            {
                this.SetValue(ref this.artistInfo, value);
                this.RaisePropertyChanged(() => this.Artist);
                this.RaisePropertyChanged(() => this.TopSongs);
                this.RaisePropertyChanged(() => this.GoogleMusicAlbums);
                this.RaisePropertyChanged(() => this.RelatedArtists);
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
                return (this.ArtistInfo == null || this.ArtistInfo.TopSongs == null) ? null : this.ArtistInfo.TopSongs.ToList();
            }
        }

        public IList<IPlaylist> GoogleMusicAlbums
        {
            get
            {
                return (this.ArtistInfo == null || this.ArtistInfo.GoogleAlbums == null) ? null : this.ArtistInfo.GoogleAlbums.Cast<IPlaylist>().ToList();
            }
        }

        public IList<IPlaylist> RelatedArtists
        {
            get
            {
                return (this.ArtistInfo == null || this.ArtistInfo.RelatedArtists == null) ? null : this.ArtistInfo.RelatedArtists.Cast<IPlaylist>().ToList();
            }
        }

        public bool IsAllAccessLoading
        {
            get
            {
                return this.isAllAccessLoading;
            }
            set
            {
                this.SetValue(ref this.isAllAccessLoading, value);
            }
        }
    }
}