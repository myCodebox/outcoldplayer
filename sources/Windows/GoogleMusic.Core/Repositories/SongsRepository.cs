// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public class SongsRepository : RepositoryBase, ISongsRepository
    {
        private readonly ILogger logger;

        public SongsRepository(
            ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("SongsRepository");
        }

        public async Task<SongBindingModel> GetSongAsync(string songId)
        {
            return new SongBindingModel(await this.Connection.GetAsync<SongEntity>(songId));
        }

        public async Task<IList<SongBindingModel>> GetAllAsync()
        {
            return (await this.Connection.Table<SongEntity>().ToListAsync()).Select(x => new SongBindingModel(x)).ToList();
        }
    }
}