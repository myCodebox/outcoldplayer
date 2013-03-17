// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IGenresRepository
    {
        Task<int> GetCountAsync();

        Task<IList<Genre>> GetGenresAsync(Order order, uint? take = null);

        Task<IList<Genre>> SearchAsync(string searchQuery, uint? take);
    }

    public class GenresRepository : RepositoryBase, IGenresRepository
    {
        private const string SqlCountGenres = @"
select count(distinct x.[GenreNorm]) from [Song] x
";

        private const string SqlAllGenres = @"
select 
       x.[GenreNorm] as [TitleNorm],
       x.[Genre] as [Title],       
       count(*) as [SongsCount],    
       sum(x.[Duration]) as [Duration],       
       x.[AlbumArtUrl] as [AlbumArtUrl],    
       max(x.[LastPlayed]) as [LastPlayed]
from [Song] x
group by x.[GenreNorm]
";

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

        private readonly Dictionary<Order, string> orderStatements = new Dictionary<Order, string>()
                                                                {
                                                                    { Order.Name,  " order by x.[GenreNorm]" },
                                                                    { Order.LastPlayed,  " order by x.[LastPlayed] desc" }
                                                                };

        public async Task<int> GetCountAsync()
        {
            return await this.Connection.ExecuteScalarAsync<int>(SqlCountGenres);
        }

        public async Task<IList<Genre>> GetGenresAsync(Order order, uint? take = null)
        {
            if (!this.orderStatements.ContainsKey(order))
            {
                throw new ArgumentOutOfRangeException("order");
            }

            var sql = new StringBuilder(SqlAllGenres);
            sql.Append(this.orderStatements[order]);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Genre>(sql.ToString());
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
