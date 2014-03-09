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

        public ArtistInfo ArtistInfo
        {
            get
            {
                return this.artistInfo;
            }

            set
            {
                this.SetValue(ref this.artistInfo, value);
                this.RaisePropertyChanged(() => this.TopSongsLimited);
                this.RaisePropertyChanged(() => this.GoogleMusicAlbumsLimited);
                this.RaisePropertyChanged(() => this.RelatedArtistsLimited);
                this.RaisePropertyChanged(() => this.TopSongsCount);
                this.RaisePropertyChanged(() => this.GoogleMusicAlbumsCount);
                this.RaisePropertyChanged(() => this.RelatedArtistsCount);
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

        public IList<Song> TopSongsLimited
        {
            get
            {
                return (this.ArtistInfo == null || this.ArtistInfo.TopSongs == null) ? null : this.ArtistInfo.TopSongs.Take(5).ToList();
            }
        }

        public IList<IPlaylist> GoogleMusicAlbumsLimited
        {
            get
            {
                return (this.ArtistInfo == null || this.ArtistInfo.GoogleAlbums == null) ? null : this.ArtistInfo.GoogleAlbums.Take(4).Cast<IPlaylist>().ToList();
            }
        }

        public IList<IPlaylist> RelatedArtistsLimited
        {
            get
            {
                return (this.ArtistInfo == null || this.ArtistInfo.RelatedArtists == null) ? null : this.ArtistInfo.RelatedArtists.Take(4).Cast<IPlaylist>().ToList();
            }
        }

        public int TopSongsCount
        {
            get
            {
                return (this.ArtistInfo == null || this.ArtistInfo.TopSongs == null) ? 0 : this.ArtistInfo.TopSongs.Count;
            }
        }

        public int GoogleMusicAlbumsCount
        {
            get
            {
                return (this.ArtistInfo == null || this.ArtistInfo.GoogleAlbums == null) ? 0 : this.ArtistInfo.GoogleAlbums.Count;
            }
        }

        public int RelatedArtistsCount
        {
            get
            {
                return (this.ArtistInfo == null || this.ArtistInfo.RelatedArtists == null) ? 0 : this.ArtistInfo.RelatedArtists.Count;
            }
        }
    }
}