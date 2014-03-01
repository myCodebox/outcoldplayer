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

    public interface IGenresRepository : IPlaylistRepository<Genre>
    {
    }

    public class GenresRepository : RepositoryBase, IGenresRepository
    {
        private const string SqlSearchGenres = @"
select x.*
from [Genre] as x  
where (?1 = 1 or x.[OfflineSongsCount] > 0) and x.[TitleNorm] like ?2
order by x.[TitleNorm]
";

        private const string SqlGenreSongs = @"
select s.* 
from [Song] as s
     inner join Genre g on s.GenreTitleNorm = g.TitleNorm
where (?1 = 1 or s.[IsCached] = 1) and s.IsLibrary = 1 and g.GenreId = ?2
order by coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]), s.AlbumTitleNorm, coalesce(nullif(s.Disc, 0), 1), s.Track
";
        
        private readonly IApplicationStateService stateService;

        public GenresRepository(IApplicationStateService stateService)
        {
            this.stateService = stateService;
        }

        public async Task<int> GetCountAsync()
        {
            var query = this.Connection.Table<Genre>();

            if (this.stateService.IsOffline())
            {
                query = query.Where(a => a.OfflineSongsCount > 0);
            }

            return await query.CountAsync();
        }

        public async Task<IList<Genre>> GetAllAsync(Order order, uint? take = null)
        {
            var query = this.Connection.Table<Genre>();

            if (this.stateService.IsOffline())
            {
                query = query.Where(a => a.OfflineSongsCount > 0);
            }

            if (order == Order.Name)
            {
                query = query.OrderBy(g => g.TitleNorm);
            }
            else if (order == Order.LastPlayed)
            {
                query = query.OrderByDescending(g => g.Recent);
            }

            if (take.HasValue)
            {
                query = query.Take((int)take.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IList<Genre>> SearchAsync(string searchQuery, uint? take)
        {
            var searchQueryNorm = searchQuery.Normalize() ?? string.Empty;

            var sql = new StringBuilder(SqlSearchGenres);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Genre>(sql.ToString(), this.stateService.IsOnline(), string.Format("%{0}%", searchQueryNorm));
        }

        public async Task<Genre> GetAsync(string id)
        {
            int genreId = int.Parse(id);

            var query = this.Connection.Table<Genre>().Where(a => a.GenreId == genreId);

            if (this.stateService.IsOffline())
            {
                query = query.Where(a => a.OfflineSongsCount > 0);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IList<Song>> GetSongsAsync(string id, bool includeAll = false)
        {
            return await this.Connection.QueryAsync<Song>(SqlGenreSongs, includeAll || this.stateService.IsOnline(), id);
        }
    }
}
