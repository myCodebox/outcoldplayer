// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public enum Order
    {
        Name = 0,

        LastPlayed = 1
    }

    public interface ISongsService
    {
        Task<List<Album>> GetAllAlbumsAsync(Order order = Order.Name);

        Task<List<MusicPlaylist>> GetAllPlaylistsAsync(Order order = Order.Name);

        Task<List<Genre>> GetAllGenresAsync(Order order = Order.Name);

        Task<List<Artist>> GetAllArtistsAsync(Order order = Order.Name);

        Task<MusicPlaylist> CreatePlaylistAsync();

        Task<bool> DeletePlaylistAsync(MusicPlaylist playlist);

        Task<bool> RemoveSongFromPlaylistAsync(MusicPlaylist playlist, int index);

        Task<bool> AddSongToPlaylistAsync(MusicPlaylist playlist, Song song);

        Task<bool> ChangePlaylistNameAsync(MusicPlaylist playlist, string newName);
    }
}