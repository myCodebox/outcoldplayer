// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;

    public interface IUserPlaylistRepository
    {
        Task<IEnumerable<UserPlaylist>> GetAllAsync();

        Task<UserPlaylist> CreateAsync(string name);

        Task<bool> DeleteAsync(string playlistId);

        Task<bool> ChangeName(string playlistId, string name);

        Task<bool> RemoveEntry(string playlistId, string songId, string entryId);

        Task<bool> AddEntriesAsync(string playlistId, List<Song> song);
    }
}