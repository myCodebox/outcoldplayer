// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;

    public interface IArtistsRepository : IPlaylistRepository<Artist>
    {
    }

    public class ArtistsRepository : RepositoryBase, IArtistsRepository
    {
        private const string SqlSearchArtist = @"
select x.*
from [Artist] as x  
where (?1 = 1 or x.[OfflineSongsCount] > 0) and x.[TitleNorm] like ?2
order by x.[TitleNorm]
";

        private const string SqlArtistSongs = @"
select * 
from
(
select s.*,
       0 as [IsCollection]
from [Song] as s
     inner join Album a on s.AlbumTitleNorm  = a.TitleNorm and coalesce(nullif(s.[AlbumArtistTitleNorm], ''), s.[ArtistTitleNorm]) = a.[ArtistTitleNorm]
     inner join Artist ta on a.[ArtistTitleNorm] = ta.[TitleNorm]
where  (?1 = 1 or s.[IsCached] = 1) and s.IsLibrary = 1 and ta.ArtistId = ?2

union 

select s.*,
       1 as [IsCollection]
from [Song] as s    
    inner join [Artist] ar on ar.[TitleNorm] = s.[ArtistTitleNorm]
where (?1 = 1 or s.[IsCached] = 1) and s.IsLibrary = 1 and s.[ArtistTitleNorm] <> coalesce(nullif(s.[AlbumArtistTitleNorm], ''), s.[ArtistTitleNorm]) and ar.[ArtistId] = ?2
) as x
order by x.IsCollection, x.Year, x.[AlbumTitleNorm], coalesce(nullif(x.Disc, 0), 1), x.Track 
";

        private readonly IApplicationStateService stateService;

        public ArtistsRepository(IApplicationStateService stateService)
        {
            this.stateService = stateService;
        }

        public async Task<IList<Artist>> GetAllAsync(Order order, uint? take = null)
        {
            var query = this.Connection.Table<Artist>().Where(a => a.AlbumsCount > 0);

            if (this.stateService.IsOffline())
            {
                query = query.Where(a => a.OfflineSongsCount > 0);
            }

            if (order == Order.Name)
            {
                query = query.OrderBy(x => x.TitleNorm);
            }
            else if (order == Order.LastPlayed)
            {
                query = query.OrderByDescending(x => x.LastPlayed);
            }

            if (take.HasValue)
            {
                query = query.Take((int)take.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<int> GetCountAsync()
        {
            var query = this.Connection.Table<Artist>().Where(a => a.AlbumsCount > 0);

            if (this.stateService.IsOffline())
            {
                query = query.Where(a => a.OfflineSongsCount > 0);
            }

            return await query.CountAsync();
        }

        public async Task<IList<Artist>> SearchAsync(string searchQuery, uint? take)
        {
            var searchQueryNorm = searchQuery.Normalize() ?? string.Empty;

            var sql = new StringBuilder(SqlSearchArtist);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Artist>(sql.ToString(), this.stateService.IsOnline(), string.Format("%{0}%", searchQueryNorm));
        }

        public async Task<Artist> GetAsync(string id)
        {
            int artistId = int.Parse(id);

            var query = this.Connection.Table<Artist>().Where(a => a.ArtistId == artistId);

            if (this.stateService.IsOffline())
            {
                query = query.Where(a => a.OfflineSongsCount > 0);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IList<Song>> GetSongsAsync(string id, bool includeAll = false)
        {
            return await this.Connection.QueryAsync<Song>(SqlArtistSongs, includeAll || this.stateService.IsOnline(), id);
        }
    }
}
