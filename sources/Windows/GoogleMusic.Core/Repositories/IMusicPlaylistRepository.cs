// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IMusicPlaylistRepository
    {
        Task InitializeAsync();

        Task<IEnumerable<MusicPlaylist>> GetAllAsync();

        Task<MusicPlaylist> CreateAsync(string name);

        Task<bool> DeleteAsync(string playlistId);

        Task<bool> ChangeName(string playlistId, string name);

        Task<bool> RemoveEntry(string playlistId, string entryId);

        Task<bool> AddEntriesAsync(string playlistId, List<Song> song);

        void ClearRepository();
    }
}