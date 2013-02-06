// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface ISongsService
    {
        Task<List<MusicPlaylist>> GetAllPlaylistsAsync(Order order = Order.None, bool canReload = false);

        Task<MusicPlaylist> CreatePlaylistAsync();

        Task<List<Song>> GetAllGoogleSongsAsync(IProgress<int> progress = null);

        Task<List<SystemPlaylist>> GetSystemPlaylists();

        Task<bool> DeletePlaylistAsync(MusicPlaylist playlist);

        Task<bool> RemoveSongFromPlaylistAsync(MusicPlaylist playlist, int index);

        Task<bool> AddSongToPlaylistAsync(MusicPlaylist playlist, Song song);

        Task<bool> ChangePlaylistNameAsync(MusicPlaylist playlist, string newName);
    }
}