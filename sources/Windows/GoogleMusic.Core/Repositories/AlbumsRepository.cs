// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IAlbumsRepository : IPlaylistRepository<Album>
    {
        Task<IList<Album>> GetArtistAlbumsAsync(int atistId);

        Task<Album> FindSongAlbumAsync(int songId);
    }

    public class AlbumsRepository : RepositoryBase, IAlbumsRepository
    {
        private const string SqlAllAlbums = @"
select 
       x.[AlbumId],
       x.[Title],  
       x.[TitleNorm],
       x.[ArtistTitleNorm],       
       x.[GenreTitleNorm],
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
     inner join [Artist] as a on x.[ArtistTitleNorm] = a.[TitleNorm]      
";

       private const string SqlSearchAlbums = @"
select 
       x.[AlbumId],
       x.[Title],  
       x.[TitleNorm],
       x.[ArtistTitleNorm],       
       x.[GenreTitleNorm],
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
     inner join [Artist] as a on x.[ArtistTitleNorm] = a.[TitleNorm]
where x.[TitleNorm] like ?1
order by x.[TitleNorm]
";

        // TODO: We need to include here also not-artist albums but which contain artist songs
        private const string SqlArtistAlbums = @"
select *
from
(
select 
       x.[AlbumId],
       x.[Title],  
       x.[TitleNorm],
       x.[ArtistTitleNorm],       
       x.[GenreTitleNorm],
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
       a.[LastPlayed]  as [Artist.LastPlayed],    
       0 as [IsCollection]
from [Album] x 
     inner join [Artist] as a on x.[ArtistTitleNorm] = a.[TitleNorm]     
where a.[ArtistId] = ?1

union

select 
       a.[AlbumId],
       a.[Title],  
       a.[TitleNorm],
       a.[GenreTitleNorm],       
       a.[ArtistTitleNorm],
       count(distinct s.SongId) as [SongsCount], 
       a.[Year],    
       sum(s.[Duration]) as [Duration],       
       a.[ArtUrl],    
       a.[LastPlayed],       
       ar.[ArtistId] as [Artist.ArtistId],
       ar.[Title] as [Artist.Title],
       ar.[TitleNorm] as [Artist.TitleNorm],
       ar.[AlbumsCount] as [Artist.AlbumsCount],
       ar.[SongsCount] as [Artist.SongsCount],
       ar.[Duration] as [Artist.Duration],
       ar.[ArtUrl] as [Artist.ArtUrl],
       ar.[LastPlayed]  as [Artist.LastPlayed],       
       1 as [IsCollection]
from [Song] as s 
    inner join [Album] a on s.[AlbumTitleNorm] = a.[TitleNorm]      
    inner join [Artist] ar on ar.[TitleNorm] = s.[ArtistTitleNorm]
where s.[ArtistTitleNorm] <> s.[AlbumArtistTitleNorm] and ar.[ArtistId] = ?1
group by a.[AlbumId], a.[Title], a.[TitleNorm], a.[ArtistTitleNorm], a.[Year], a.[ArtUrl], a.[LastPlayed]
) as x
order by x.IsCollection, x.Year 
";

        private const string SqlAlbumsSongs = @"
select s.* 
from [Song] as s
     inner join Album a on s.[AlbumTitleNorm] = a.[TitleNorm] and coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = a.[ArtistTitleNorm]
where a.AlbumId = ?1
order by coalesce(nullif(s.Disc, 0), 1), s.Track
";

        private const string SqlSongAlbum = @"
select 
       x.[AlbumId],
       x.[Title],  
       x.[TitleNorm],
       x.[ArtistTitleNorm],       
       x.[GenreTitleNorm],
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
     inner join [Artist] as a on x.[ArtistTitleNorm] = a.[TitleNorm]
     inner join [Song] as s on x.[AlbumTitleNorm] = s.[AlbumTitleNorm]
where s.[SongId] = ?1
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

        public async Task<IList<Album>> GetAllAsync(Order order, uint? take = null)
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

        public async Task<IList<Album>> GetArtistAlbumsAsync(int atistId)
        {
            return await this.Connection.QueryAsync<Album>(SqlArtistAlbums, atistId);
        }

        public async Task<Album> FindSongAlbumAsync(int songId)
        {
            return (await this.Connection.QueryAsync<Album>(SqlSongAlbum, songId)).FirstOrDefault();
        }

        public async Task<IList<Album>> SearchAsync(string searchQuery, uint? take)
        {
            var searchQueryNorm = searchQuery.Normalize() ?? string.Empty;

            var sql = new StringBuilder(SqlSearchAlbums);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Album>(sql.ToString(), string.Format("%{0}%", searchQueryNorm));
        }

        public async Task<Album> GetAsync(int id)
        {
            var sql = new StringBuilder(SqlAllAlbums);
            sql.Append(" where x.[AlbumId] == ?1 ");

            return (await this.Connection.QueryAsync<Album>(sql.ToString(), id)).FirstOrDefault();
        }

        public async Task<IList<Song>> GetSongsAsync(int id)
        {
            return await this.Connection.QueryAsync<Song>(SqlAlbumsSongs, id);
        }
    }
}
