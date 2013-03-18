// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public interface IPlaylistRepository<TPlaylist> where TPlaylist : IPlaylist
    {
        Task<int> GetCountAsync();

        Task<IList<TPlaylist>> GetAllAsync(Order order, uint? take = null);

        Task<IList<TPlaylist>> SearchAsync(string searchQuery, uint? take);

        Task<TPlaylist> GetAsync(int id);

        Task<IList<Song>> GetSongsAsync(int id);
    }
}
