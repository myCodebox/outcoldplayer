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
        Task<IList<SongBindingModel>> GetAllAsync();

        Task<SongBindingModel> GetSongAsync(string songId);
    }
}