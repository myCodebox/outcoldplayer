// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using OutcoldSolutions.GoogleMusic.Models;

    public interface IPlaylistCollectionsService
    {
        IPlaylistCollection<TPlaylist> GetCollection<TPlaylist>() where TPlaylist : Playlist;
    }
}