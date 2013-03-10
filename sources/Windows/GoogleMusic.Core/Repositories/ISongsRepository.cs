// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public interface ISongsRepository
    {
        event Action Updated;

        Task InitializeAsync(IProgress<int> progress);

        Task<IEnumerable<Song>> GetAllAsync();

        Task<Song> GetSongAsync(string songId);

        Task ClearRepositoryAsync();
    }
}