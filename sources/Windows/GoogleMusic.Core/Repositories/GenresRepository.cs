// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IGenresRepository : IPlaylistRepository<Genre>
    {
        Task<Uri[]> GetUrisAsync(string titleNorm);
    }

    public class GenresRepository : RepositoryBase, IGenresRepository
    {
        private const string SqlSearchGenres = @"
select x.*
from [Genre] as x  
where (?1 = 1 or x.[OfflineSongsCount] > 0) and x.[TitleNorm] like ?2
order by x.[TitleNorm]
";

        private const string SqlAllGenres = @"
select x.*
from [Genre] as x  
";

        private const string SqlGenreSongs = @"
select s.* 
from [Song] as s
     inner join Genre g on s.GenreTitleNorm = g.TitleNorm
where (?1 = 1 or s.[IsCached] = 1) and s.IsLibrary = 1 and g.GenreId = ?2
order by coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]), s.AlbumTitleNorm, coalesce(nullif(s.Disc, 0), 1), s.Track
";

        private const string SqlGetUris = @"
select *
from
(
 select distinct(s.AlbumArtUrl)  as Url
 from Song s 
 where s.AlbumArtUrl is not null and (?1 = 1 or s.[IsCached] = 1) and s.GenreTitleNorm = ?2
 order by s.Recent desc
)
limit 4
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
            StringBuilder query = new StringBuilder(SqlAllGenres);

            if (this.stateService.IsOffline())
            {
                query.Append(" where x.OfflineSongsCount > 0 ");
            }

            if (order == Order.Name)
            {
                query = query.Append(" order by x.TitleNorm");
            }
            else if (order == Order.LastPlayed)
            {
                query = query.Append(" order by x.Recent");
            }

            if (take.HasValue)
            {
                query = query.AppendFormat(CultureInfo.InvariantCulture, " limit {0}", take.Value);
            }

            return await Connection.QueryAsync<Genre>(query.ToString(), this.stateService.IsOnline());
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

        public async Task<Uri[]> GetUrisAsync(string titleNorm)
        {
            return (await this.Connection.QueryAsync<UrlRef>(SqlGetUris, this.stateService.IsOnline(), titleNorm)).Select(x => new Uri(x.Url)).ToArray();
        }
    }
}
