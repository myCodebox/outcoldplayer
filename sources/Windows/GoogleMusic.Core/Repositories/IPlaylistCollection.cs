// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public interface IPlaylistCollection<TPlaylist> where TPlaylist : Playlist 
    {
        Task<int> CountAsync();

        Task<IEnumerable<TPlaylist>> GetAllAsync(Order order = Order.Name, int takeCount = int.MaxValue);

        Task<IEnumerable<TPlaylist>> SearchAsync(string query, int takeCount = int.MaxValue);
    }
}