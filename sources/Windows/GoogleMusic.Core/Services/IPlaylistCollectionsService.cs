// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;

    public interface IPlaylistCollectionsService
    {
        IPlaylistCollection<TPlaylist> GetCollection<TPlaylist>() where TPlaylist : Playlist;
    }
}