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

        Task<List<Playlist>> GetAllPlaylistsAsync(Order order = Order.Name);
    }
}