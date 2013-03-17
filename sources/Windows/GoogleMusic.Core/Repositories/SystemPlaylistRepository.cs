// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public interface ISystemPlaylistRepository
    {
        Task<SystemPlaylist> GetHighlyRatedPlaylistAsync();

        Task<SystemPlaylist> GetLastAddedSongsPlaylistAsync();

        Task<SystemPlaylist> GetAllSongsPlaylistAsync();

        Task<IList<SystemPlaylist>> GetSystemPlaylistsAsync();
    }

    public class SystemPlaylistRepository : RepositoryBase, ISystemPlaylistRepository
    {
        private const int HighlyRatedValue = 4;
        private const int LastAddedSongsCount = 500;

        private const string SqlHiglyRatedSongsPlaylits = @"
select count(*) as SongsCount, sum(s.[Duration]) as Duration, ?2 as [SystemPlaylistType]
from [Song] as s
where s.[Rating] >= ?1 
";

        private const string SqlLastAddedPlaylist = @"
select count(*) as SongsCount, sum(x.[Duration]) as Duration, ?2 as [SystemPlaylistType] from 
(
  select *
  from [Song] as s  
  order by s.[CreationDate]
  limit ?1
) as x
";

        private const string SqlAllSongsPlaylist = @"
select count(*) as SongsCount, sum(s.[Duration]) as Duration, ?1 as [SystemPlaylistType] from [Song] as s
";

        public async Task<SystemPlaylist> GetHighlyRatedPlaylistAsync()
        {
            return (await this.Connection.QueryAsync<SystemPlaylist>(SqlHiglyRatedSongsPlaylits, HighlyRatedValue, SystemPlaylistType.HighlyRated)).First();
        }

        public async Task<SystemPlaylist> GetLastAddedSongsPlaylistAsync()
        {
            return (await this.Connection.QueryAsync<SystemPlaylist>(SqlLastAddedPlaylist, LastAddedSongsCount, SystemPlaylistType.LastAdded)).First();
        }

        public async Task<SystemPlaylist> GetAllSongsPlaylistAsync()
        {
            return (await this.Connection.QueryAsync<SystemPlaylist>(SqlAllSongsPlaylist, SystemPlaylistType.AllSongs)).First();
        }

        public async Task<IList<SystemPlaylist>> GetSystemPlaylistsAsync()
        {
            return await Task.WhenAll(
                this.GetAllSongsPlaylistAsync(),
                this.GetHighlyRatedPlaylistAsync(),
                this.GetLastAddedSongsPlaylistAsync()
                );
        }
    }
}
