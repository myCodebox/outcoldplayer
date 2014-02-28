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
    using OutcoldSolutions.GoogleMusic.Services;

    public interface IAlbumsRepository : IPlaylistRepository<Album>
    {
        Task<IList<Album>> GetArtistAlbumsAsync(string artistId);

        Task<Album> FindSongAlbumAsync(string songId);
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
       x.[OfflineSongsCount],
       x.[OfflineDuration],
       x.[GoogleAlbumId],
       a.[ArtistId] as [Artist.ArtistId],
       a.[Title] as [Artist.Title],
       a.[TitleNorm] as [Artist.TitleNorm],
       a.[AlbumsCount] as [Artist.AlbumsCount],
       a.[SongsCount] as [Artist.SongsCount],
       a.[Duration] as [Artist.Duration],
       a.[ArtUrl] as [Artist.ArtUrl],
       a.[LastPlayed]  as [Artist.LastPlayed],
       a.[OfflineSongsCount]  as [Artist.OfflineSongsCount],
       a.[OfflineDuration]  as [Artist.OfflineDuration],
       a.[GoogleArtistId] as [Artist.GoogleArtistId]
from [Album] x 
     inner join [Artist] as a on x.[ArtistTitleNorm] = a.[TitleNorm]  
where (?1 = 1 or x.[OfflineSongsCount] > 0) 
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
       x.[OfflineSongsCount],
       x.[OfflineDuration],
       x.[GoogleAlbumId],
       a.[ArtistId] as [Artist.ArtistId],
       a.[Title] as [Artist.Title],
       a.[TitleNorm] as [Artist.TitleNorm],
       a.[AlbumsCount] as [Artist.AlbumsCount],
       a.[SongsCount] as [Artist.SongsCount],
       a.[Duration] as [Artist.Duration],
       a.[ArtUrl] as [Artist.ArtUrl],
       a.[LastPlayed]  as [Artist.LastPlayed],
       a.[OfflineSongsCount]  as [Artist.OfflineSongsCount],
       a.[OfflineDuration]  as [Artist.OfflineDuration],
       a.[GoogleArtistId] as [Artist.GoogleArtistId]
from [Album] x 
     inner join [Artist] as a on x.[ArtistTitleNorm] = a.[TitleNorm]
where (?1 = 1 or x.[OfflineSongsCount] > 0) and x.[TitleNorm] like ?2
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
       x.[OfflineSongsCount],
       x.[OfflineDuration],
       x.[GoogleAlbumId],
       a.[ArtistId] as [Artist.ArtistId],
       a.[Title] as [Artist.Title],
       a.[TitleNorm] as [Artist.TitleNorm],
       a.[AlbumsCount] as [Artist.AlbumsCount],
       a.[SongsCount] as [Artist.SongsCount],
       a.[Duration] as [Artist.Duration],
       a.[ArtUrl] as [Artist.ArtUrl],
       a.[LastPlayed]  as [Artist.LastPlayed],
       a.[OfflineSongsCount]  as [Artist.OfflineSongsCount],
       a.[OfflineDuration]  as [Artist.OfflineDuration],
       a.[GoogleArtistId] as [Artist.GoogleArtistId],
       0 as [IsCollection]
from [Album] x 
     inner join [Artist] as a on x.[ArtistTitleNorm] = a.[TitleNorm]     
where (?1 = 1 or x.[OfflineSongsCount] > 0) and a.[ArtistId] = ?2

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
       a.[OfflineSongsCount],
       a.[OfflineDuration],
       a.[GoogleAlbumId],
       ar.[ArtistId] as [Artist.ArtistId],
       ar.[Title] as [Artist.Title],
       ar.[TitleNorm] as [Artist.TitleNorm],
       ar.[AlbumsCount] as [Artist.AlbumsCount],
       ar.[SongsCount] as [Artist.SongsCount],
       ar.[Duration] as [Artist.Duration],
       ar.[ArtUrl] as [Artist.ArtUrl],
       ar.[LastPlayed]  as [Artist.LastPlayed],
       ar.[OfflineSongsCount]  as [Artist.OfflineSongsCount],
       ar.[OfflineDuration]  as [Artist.OfflineDuration],
       ar.[GoogleArtistId] as [Artist.GoogleArtistId],
       1 as [IsCollection]
from [Song] as s 
    inner join [Album] a on s.[AlbumTitleNorm] = a.[TitleNorm]      
    inner join [Artist] ar on ar.[TitleNorm] = s.[ArtistTitleNorm]
where (?1 = 1 or s.IsCached = 1) and
    s.IsLibrary = 1 and
    s.[ArtistTitleNorm] <> coalesce(nullif(s.[AlbumArtistTitleNorm], ''), s.[ArtistTitleNorm]) and ar.[ArtistId] = ?2
group by a.[AlbumId], a.[Title], a.[TitleNorm], a.[ArtistTitleNorm], a.[Year], a.[ArtUrl], a.[LastPlayed]
) as x
order by x.IsCollection, x.Year 
";

        private const string SqlAlbumsSongs = @"
select s.* 
from [Song] as s
     inner join Album a on s.[AlbumTitleNorm] = a.[TitleNorm] and coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = a.[ArtistTitleNorm]
where  (?1 = 1 or s.[IsCached] = 1) and s.IsLibrary = 1 and a.AlbumId = ?2
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
       x.[OfflineSongsCount],
       x.[OfflineDuration],
       x.[GoogleAlbumId],
       a.[ArtistId] as [Artist.ArtistId],
       a.[Title] as [Artist.Title],
       a.[TitleNorm] as [Artist.TitleNorm],
       a.[AlbumsCount] as [Artist.AlbumsCount],
       a.[SongsCount] as [Artist.SongsCount],
       a.[Duration] as [Artist.Duration],
       a.[ArtUrl] as [Artist.ArtUrl],
       a.[LastPlayed]  as [Artist.LastPlayed],
       a.[OfflineSongsCount]  as [Artist.OfflineSongsCount],
       a.[OfflineDuration]  as [Artist.OfflineDuration],
       a.[GoogleArtistId] as [Artist.GoogleArtistId]
from [Album] x 
     inner join [Artist] as a on x.[ArtistTitleNorm] = a.[TitleNorm]
     inner join [Song] as s on x.[TitleNorm] = s.[AlbumTitleNorm] and coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = x.[ArtistTitleNorm]
where (?1 = 1 or x.[OfflineSongsCount] > 0) and s.IsLibrary = 1 and s.[SongId] = ?2
";

        private const string SqlAlbumCount = @"select count(*) from [Album] x where ?1 = 1 or x.[OfflineSongsCount] > 0";

        private static readonly Dictionary<Order, string> OrderStatements = new Dictionary<Order, string>()
                                                                {
                                                                    { Order.Name,  " order by x.[TitleNorm]" },
                                                                    { Order.LastPlayed,  " order by x.[LastPlayed] desc" }
                                                                };

        private readonly IApplicationStateService stateService;

        public AlbumsRepository(IApplicationStateService stateService)
        {
            this.stateService = stateService;
        }

        public async Task<int> GetCountAsync()
        {
            return await this.Connection.ExecuteScalarAsync<int>(SqlAlbumCount, this.stateService.IsOnline());
        }

        public async Task<IList<Album>> GetAllAsync(Order order, uint? take = null)
        {
            if (!OrderStatements.ContainsKey(order))
            {
                throw new ArgumentOutOfRangeException("order");
            }

            var sql = new StringBuilder(SqlAllAlbums);
            sql.Append(OrderStatements[order]);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Album>(sql.ToString(), this.stateService.IsOnline());
        }

        public async Task<IList<Album>> GetArtistAlbumsAsync(string artistId)
        {
            return await this.Connection.QueryAsync<Album>(SqlArtistAlbums, this.stateService.IsOnline(), artistId);
        }

        public async Task<Album> FindSongAlbumAsync(string songId)
        {
            return (await this.Connection.QueryAsync<Album>(SqlSongAlbum, this.stateService.IsOnline(), songId)).FirstOrDefault();
        }

        public async Task<IList<Album>> SearchAsync(string searchQuery, uint? take)
        {
            var searchQueryNorm = searchQuery.Normalize() ?? string.Empty;

            var sql = new StringBuilder(SqlSearchAlbums);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Album>(sql.ToString(), this.stateService.IsOnline(), string.Format("%{0}%", searchQueryNorm));
        }

        public async Task<Album> GetAsync(string id)
        {
            var sql = new StringBuilder(SqlAllAlbums);
            sql.Append(" and x.[AlbumId] == ?2 ");

            return (await this.Connection.QueryAsync<Album>(sql.ToString(), this.stateService.IsOnline(), id)).FirstOrDefault();
        }

        public async Task<IList<Song>> GetSongsAsync(string id, bool includeAll = false)
        {
            return await this.Connection.QueryAsync<Song>(SqlAlbumsSongs, includeAll || this.stateService.IsOnline(), id);
        }
    }
}
