// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;

    public interface ISongsRepository
    {
        Task<IEnumerable<Song>> GetAllAsync();

        Task<Song> GetSongAsync(string songId);
    }
}