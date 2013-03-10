// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class SongsRepository : RepositoryBase, ISongsRepository
    {
        private readonly ILogger logger;

        public SongsRepository(
            ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("SongsRepository");
        }

        public async Task<Song> GetSongAsync(string songId)
        {
            return new Song(await this.Connection.GetAsync<SongEntity>(songId));
        }

        public async Task<IEnumerable<Song>> GetAllAsync()
        {
            return (await this.Connection.Table<SongEntity>().ToListAsync()).Select(x => new Song(x));
        }
    }
}