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
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public interface IAlbumsRepository
    {
        Task<int> GetCountAsync();

        Task<IList<Album>> GetAlbumsAsync(Order order, uint? take = null);

        Task<IList<Album>> GetArtistAlbumsAsync(string artistNorm);

        Task<IList<Album>> SearchAsync(string searchQuery, uint? take);
    }

    public class AlbumsRepository : RepositoryBase, IAlbumsRepository
    {
        private const string SqlAllAlbums = @"
select 
       x.[AlbumId],
       x.[Title],  
       x.[TitleNorm],
       x.[ArtistId],
       x.[SongsCount], 
       x.[Year],    
       x.[Duration],       
       x.[ArtUrl],    
       x.[LastPlayed],       
       a.[ArtistId] as [Artist.ArtistId],
       a.[Title] as [Artist.Title],
       a.[TitleNorm] as [Artist.TitleNorm],
       a.[AlbumsCount] as [Artist.AlbumsCount],
       a.[SongsCount] as [Artist.SongsCount],
       a.[Duration] as [Artist.Duration],
       a.[ArtUrl] as [Artist.ArtUrl],
       a.[LastPlayed]  as [Artist.LastPlayed]
from [Album] x 
     inner join [Artist] as a on x.[ArtistId] = a.[ArtistId]    
";

       private const string SqlSearchAlbums = @"
select 
       x.[ArtistNormX] as [ArtistNorm], 
       x.[AlbumNorm] as [TitleNorm],
       max(coalesce(nullif(x.[AlbumArtist], ''), x.[Artist])) as [Artist],       
       max(ifnull(x.[Album], 0)) as [Title],        
       count(*) as [SongsCount], 
       max(ifnull(x.[Year], 0)) as [Year],       
       max(ifnull(x.[Genre], '')) as [Genre],  
       max(ifnull(x.[GenreNorm], '')) as [GenreNorm],     
       sum(x.[Duration]) as [Duration],       
       max(ifnull(x.[AlbumArtUrl], '')) as [AlbumArtUrl],    
       max(x.[LastPlayed]) as [LastPlayed]
from
(
select coalesce(nullif(s.[AlbumArtistNorm], ''), s.[ArtistNorm]) as [ArtistNormX], s.*
from [Song] s
where x.[AlbumNorm] like ?1
) as x
group by x.[ArtistNormX], x.[TitleNorm]
";

        private const string SqlArtistAlbums = @"
select * from
(
select 
       x.[ArtistNormX] as [ArtistNorm], 
       x.[AlbumNorm] as [TitleNorm],
       max(coalesce(nullif(x.[AlbumArtist], ''), x.[Artist])) as [Artist],       
       max(ifnull(x.[Album], 0)) as [Title],        
       count(*) as [SongsCount], 
       max(ifnull(x.[Year], 0)) as [Year],       
       max(ifnull(x.[Genre], '')) as [Genre],  
       max(ifnull(x.[GenreNorm], '')) as [GenreNorm],     
       sum(x.[Duration]) as [Duration],       
       max(ifnull(x.[AlbumArtUrl], '')) as [AlbumArtUrl],    
       max(x.[LastPlayed]) as [LastPlayed]
from
(
select coalesce(nullif(s.[AlbumArtistNorm], ''), s.[ArtistNorm]) as [ArtistNormX], s.*
from [Song] s
where coalesce(nullif(s.[AlbumArtistNorm], ''), s.[ArtistNorm]) = ?1
) as x
group by x.[ArtistNormX], x.[AlbumNorm]

union 

select 
       x.[ArtistNormX] as [ArtistNorm], 
       x.[AlbumNorm] as [TitleNorm],
       max(coalesce(nullif(x.[AlbumArtist], ''), x.[Artist])) as [Artist],       
       max(ifnull(x.[Album], 0)) as [Title],        
       count(*) as [SongsCount], 
       max(ifnull(x.[Year], 0)) as [Year],       
       max(ifnull(x.[Genre], '')) as [Genre], 
       max(ifnull(x.[GenreNorm], '')) as [GenreNorm],      
       sum(x.[Duration]) as [Duration],       
       max(ifnull(x.[AlbumArtUrl], '')) as [AlbumArtUrl],    
       max(x.[LastPlayed]) as [LastPlayed]
from
(
select coalesce(nullif(s.[AlbumArtistNorm], ''), s.[ArtistNorm]) as [ArtistNormX], s.*
from [Song] s
where ifnull(s.[AlbumArtistNorm], '') <> '' and ifnull(s.[ArtistNorm], '') <> '' and s.[ArtistNorm] <> s.[AlbumArtistNorm]  
      and s.[ArtistNorm] = ?1
) as x
group by x.[ArtistNormX], x.[AlbumNorm]
) as u
order by u.[Year], u.[TitleNorm]
";

        private readonly Dictionary<Order, string> orderStatements = new Dictionary<Order, string>()
                                                                {
                                                                    { Order.Name,  " order by x.[TitleNorm]" },
                                                                    { Order.LastPlayed,  " order by x.[LastPlayed] desc" }
                                                                };

        public async Task<int> GetCountAsync()
        {
            return await this.Connection.Table<Album>().CountAsync();
        }

        public async Task<IList<Album>> GetAlbumsAsync(Order order, uint? take = null)
        {
            if (!this.orderStatements.ContainsKey(order))
            {
                throw new ArgumentOutOfRangeException("order");
            }

            var sql = new StringBuilder(SqlAllAlbums);
            sql.Append(this.orderStatements[order]);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Album>(sql.ToString());
        }

        public async Task<IList<Album>> GetArtistAlbumsAsync(string artistNorm)
        {
            return await this.Connection.QueryAsync<Album>(SqlArtistAlbums, artistNorm);
        }

        public async Task<IList<Album>> SearchAsync(string searchQuery, uint? take)
        {
            var searchQueryNorm = searchQuery.Normalize() ?? string.Empty;

            var sql = new StringBuilder(SqlSearchAlbums);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Album>(sql.ToString(), string.Format("%{0}%", searchQueryNorm.Normalize()));
        }
    }
}
