// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    public interface IPlaylistCollectionsService
    {
        IAlbumCollection GetAlbumCollection();

        IArtistCollection GetArtistCollection();

        IGenreCollection GetGenreCollection();
    }
}