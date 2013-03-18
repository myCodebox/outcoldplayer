// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public interface IGenresRepository
    {
        Task<int> GetCountAsync();

        Task<IList<Genre>> GetGenresAsync(Order order, uint? take = null);

        Task<IList<Genre>> SearchAsync(string searchQuery, uint? take);
    }

    public class GenresRepository : RepositoryBase, IGenresRepository
    {
        private const string SqlSearchGenres = @"
select 
       x.[GenreNorm] as [TitleNorm],
       ifnull((select i.[Genre] from [Song] as i where i.[GenreNorm] = x.[GenreNorm] and i.[Genre] <> '' limit 1), '') as [Title],       
       count(*) as [SongsCount],    
       sum(x.[Duration]) as [Duration],       
       ifnull((select i.[AlbumArtUrl] from [Song] as i where i.[GenreNorm] = x.[GenreNorm] and i.[AlbumArtUrl] <> '' limit 1), '') as [AlbumArtUrl],    
       max(x.[LastPlayed]) as [LastPlayed]
from [Song] x
where x.[GenreNorm] like ?1
group by x.[GenreNorm]
";

        public async Task<int> GetCountAsync()
        {
            return await this.Connection.Table<Genre>().CountAsync();
        }

        public async Task<IList<Genre>> GetGenresAsync(Order order, uint? take = null)
        {
            var query = this.Connection.Table<Genre>();

            if (order == Order.Name)
            {
                query = query.OrderBy(g => g.TitleNorm);
            }
            else if (order == Order.LastPlayed)
            {
                query = query.OrderByDescending(g => g.LastPlayed);
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

            return await this.Connection.QueryAsync<Genre>(sql.ToString(), string.Format("%{0}%", searchQueryNorm.Normalize()));
        }
    }
}
