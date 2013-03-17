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

    public interface IArtistsRepository
    {
        Task<int> GetCountAsync();

        Task<IList<Artist>> GetAristsAsync(Order order, uint? take = null);

        Task<IList<Artist>> SearchAsync(string searchQuery, uint? take);
    }

    public class ArtistsRepository : RepositoryBase, IArtistsRepository
    {
        private const string SqlAllArtists = @"
select 
       x.[AlbumArtistNorm] as [TitleNorm],        
       count(distinct x.[AlbumNorm]) as [AlbumsCount],       
       x.[AlbumArtist] as [Title],
       count(*) as [SongsCount], 
       x.[Duration] as [Duration],       
       x.[AlbumArtUrl] as [ArtistArtUrl],    
       x.[LastPlayed] as [LastPlayed]
from [Song] x
group by x.[AlbumArtistNorm]
";

        private const string SqlCountArtist = @"
select count(distinct x.[AlbumArtistNorm]) from [Song] x
";

        private const string SqlSearchArtist = @"
select 
       u.[ArtistNorm] as [TitleNorm],       
       sum(u.[AlbumsCount]) as [AlbumsCount],       
       max(ifnull(u.[Artist], '')) as [Title],       
       sum(u.[SongsCount]) as [SongsCount],
       sum(u.[Duration]) as [Duration],        
       max(ifnull(u.[ArtistArtUrl], '')) as [ArtistArtUrl],       
       max(u.[LastPlayed]) as [LastPlayed]
from
(
  select 
         x.[ArtistNormX] as [ArtistNorm],        
         count(distinct x.[AlbumNorm]) as [AlbumsCount],
         max(coalesce(nullif(x.[AlbumArtist], ''), x.[Artist])) as [Artist],       
         count(*) as [SongsCount], 
         sum(x.[Duration]) as [Duration],       
         max(ifnull(x.[AlbumArtUrl], '')) as [ArtistArtUrl],    
         max(x.[LastPlayed]) as [LastPlayed]
  from
  (
  select coalesce(nullif(s.[AlbumArtistNorm], ''), s.[ArtistNorm]) as [ArtistNormX], s.*
  from [Song] s 
  ) as x  
  where x.[ArtistNormX] like ?1
  group by x.[ArtistNormX]

union 

  select 
         x.[ArtistNorm], 
         0 as [AlbumsCount],
         max(ifnull(x.[Artist], '')) as [Artist],       
         count(*) as [SongsCount], 
         sum(x.[Duration]) as [Duration],       
         max(ifnull(x.[AlbumArtUrl], '')) as [ArtistArtUrl],
         max(x.[LastPlayed]) as [LastPlayed] 
  from [Song] as x
  where ifnull(x.[AlbumArtistNorm], '') <> '' and ifnull(x.[ArtistNorm], '') <> '' and x.[ArtistNorm] <> x.[AlbumArtistNorm]  
        and x.[ArtistNorm] like ?1
  group by x.[ArtistNorm]  
) as u
group by u.[ArtistNorm]
order by u.[ArtistNorm]
";

        private readonly Dictionary<Order, string> orderStatements = new Dictionary<Order, string>()
                                                                {
                                                                    { Order.Name,  " order by x.[ArtistNormX]" },
                                                                    { Order.LastPlayed,  " order by x.[LastPlayed] desc" }
                                                                };

        public async Task<IList<Artist>> GetAristsAsync(Order order, uint? take = null)
        {
            if (!this.orderStatements.ContainsKey(order))
            {
                throw new ArgumentOutOfRangeException("order");
            }

            var sql = new StringBuilder(SqlAllArtists);
            sql.Append(this.orderStatements[order]);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Artist>(sql.ToString());
        }

        public async Task<int> GetCountAsync()
        {
            return await this.Connection.ExecuteScalarAsync<int>(SqlCountArtist);
        }

        public async Task<IList<Artist>> SearchAsync(string searchQuery, uint? take)
        {
            var searchQueryNorm = searchQuery.Normalize() ?? string.Empty;

            var sql = new StringBuilder(SqlSearchArtist);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Artist>(sql.ToString(), string.Format("%{0}%", searchQueryNorm.Normalize()));
        }
    }
}
