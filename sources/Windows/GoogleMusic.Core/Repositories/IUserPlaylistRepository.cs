// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public interface IUserPlaylistRepository
    {
        Task<int> GetCountAsync();

        Task<IList<UserPlaylistEntity>> GetPlaylistsAsync(Order order, uint? take = null);

        Task<IList<UserPlaylistEntity>> SearchAsync(string searchQuery, uint? take);

        Task<IEnumerable<UserPlaylistBindingModel>> GetAllAsync();

        Task<UserPlaylistBindingModel> CreateAsync(string name);

        Task<bool> DeleteAsync(UserPlaylistEntity playlistId);

        Task<bool> ChangeName(UserPlaylistEntity playlistId, string name);

        Task<bool> RemoveEntry(UserPlaylistEntity playlistId, string songId, string entryId);

        Task<bool> AddEntriesAsync(UserPlaylistEntity playlistId, List<SongBindingModel> song);
    }
}