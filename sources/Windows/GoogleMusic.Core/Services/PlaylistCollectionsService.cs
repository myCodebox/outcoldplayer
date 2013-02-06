// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    public class PlaylistCollectionsService : IPlaylistCollectionsService
    {
        private readonly IAlbumCollection albumCollection;
        private readonly IArtistCollection artistCollection;
        private readonly IGenreCollection genreCollection;

        public PlaylistCollectionsService(
            IAlbumCollection albumCollection,
            IArtistCollection artistCollection,
            IGenreCollection genreCollection)
        {
            this.albumCollection = albumCollection;
            this.artistCollection = artistCollection;
            this.genreCollection = genreCollection;
        }

        public IAlbumCollection GetAlbumCollection()
        {
            return this.albumCollection;
        }

        public IArtistCollection GetArtistCollection()
        {
            return this.artistCollection;
        }

        public IGenreCollection GetGenreCollection()
        {
            return this.genreCollection;
        }
    }
}