// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IMusicPlaylistRepository
    {
        Task InitializeAsync();

        Task<IEnumerable<MusicPlaylist>> GetAllAsync();

        Task<MusicPlaylist> CreateAsync(string name);

        Task<bool> DeleteAsync(Guid playlistId);

        Task<bool> ChangeName(Guid playlistId, string name);

        Task<bool> RemoveEntry(Guid playlistId, string entryId);

        Task<bool> AddEntriesAsync(Guid playlistId, List<Song> song);

        void ClearRepository();
    }
}