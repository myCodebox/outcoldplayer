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
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public interface IAlbumsRepository : IPlaylistRepository<Album>
    {
        Task<IList<Album>> GetArtistAlbumsAsync(int atistId);
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
where x.[TitleNorm] like ?1
order by x.[TitleNorm]
";

        // TODO: We need to include here also not-artist albums but which contain artist songs
        private const string SqlArtistAlbums = @"
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
where a.[ArtistId] = ?1
";

        private const string SqlAlbumsSongs = @"
select s.* ,
       a.[AlbumId] as [Album.AlbumId],
       a.[Title] as [Album.Title],  
       a.[TitleNorm] as [Album.TitleNorm],
       a.[ArtistId] as [Album.ArtistId],
       a.[SongsCount] as [Album.SongsCount], 
       a.[Year] as [Album.Year],    
       a.[Duration] as [Album.Duration],       
       a.[ArtUrl] as [Album.ArtUrl],    
       a.[LastPlayed] as [Album.LastPlayed],       
       ta.[ArtistId] as [Artist.ArtistId],
       ta.[Title] as [Artist.Title],
       ta.[TitleNorm] as [Artist.TitleNorm],
       ta.[AlbumsCount] as [Artist.AlbumsCount],
       ta.[SongsCount] as [Artist.SongsCount],
       ta.[Duration] as [Artist.Duration],
       ta.[ArtUrl] as [Artist.ArtUrl],
       ta.[LastPlayed]  as [Artist.LastPlayed]
from [Song] as s
     inner join Album a on s.AlbumId = a.AlbumId
     inner join Artist ta on ta.ArtistId = s.ArtistId 
where s.AlbumId = ?1
order by coalesce(nullif(s.Disc, 0), 1), s.Track
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
