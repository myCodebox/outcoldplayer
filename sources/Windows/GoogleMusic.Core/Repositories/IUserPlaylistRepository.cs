// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public interface IUserPlaylistRepository : IPlaylistRepository<UserPlaylist>
    {
        Task<IEnumerable<UserPlaylistBindingModel>> GetAllAsync();

        Task<UserPlaylistBindingModel> CreateAsync(string name);

        Task<bool> DeleteAsync(UserPlaylist playlistId);

        Task<bool> ChangeName(UserPlaylist playlistId, string name);

        Task<bool> RemoveEntry(UserPlaylist playlistId, string songId, string entryId);

        Task<bool> AddEntriesAsync(UserPlaylist playlistId, List<SongBindingModel> song);
    }
}