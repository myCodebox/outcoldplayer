// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IArtistsRepository : IPlaylistRepository<Artist>
    {
    }

    public class ArtistsRepository : RepositoryBase, IArtistsRepository
    {
        private const string SqlSearchArtist = @"
select x.*
from [Artist] as x  
where x.[TitleNorm] like ?1
order by x.[TitleNorm]
";

        private const string SqlArtistSongs = @"
select * 
from
(
select s.*,
       a.[AlbumId] as [Album.AlbumId],
       a.[Title] as [Album.Title],  
       a.[TitleNorm] as [Album.TitleNorm],
       a.[ArtistTitleNorm] as [Album.ArtistTitleNorm],       
       a.[GenreTitleNorm] as [Album.GenreTitleNorm],
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
       ta.[LastPlayed]  as [Artist.LastPlayed],       
       0 as [IsCollection]
from [Song] as s
     inner join Album a on s.AlbumTitleNorm  = a.TitleNorm
     inner join Artist ta on a.[ArtistTitleNorm] = ta.[TitleNorm]
where ta.ArtistId = ?1

union 

select s.*,
       a.[AlbumId] as [Album.AlbumId],
       a.[Title] as [Album.Title],  
       a.[TitleNorm] as [Album.TitleNorm],
       a.[ArtistTitleNorm] as [Album.ArtistTitleNorm],       
       a.[GenreTitleNorm] as [Album.GenreTitleNorm],
       a.[SongsCount] as [Album.SongsCount], 
       a.[Year] as [Album.Year],    
       a.[Duration] as [Album.Duration],       
       a.[ArtUrl] as [Album.ArtUrl],    
       a.[LastPlayed] as [Album.LastPlayed],       
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
) as x
order by x.IsCollection, x.Year, x.[Album.TitleNorm], coalesce(nullif(x.Disc, 0), 1), x.Track 
";

        public async Task<IList<Artist>> GetAllAsync(Order order, uint? take = null)
        {
            var query = this.Connection.Table<Artist>().Where(a => a.AlbumsCount > 0);
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
            return await this.Connection.Table<Artist>().Where(a => a.AlbumsCount > 0).CountAsync();
        }

        public async Task<IList<Artist>> SearchAsync(string searchQuery, uint? take)
        {
            var searchQueryNorm = searchQuery.Normalize() ?? string.Empty;

            var sql = new StringBuilder(SqlSearchArtist);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Artist>(sql.ToString(), string.Format("%{0}%", searchQueryNorm));
        }

        public async Task<Artist> GetAsync(int id)
        {
            return await this.Connection.Table<Artist>().Where(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IList<Song>> GetSongsAsync(int id)
        {
            return await this.Connection.QueryAsync<Song>(SqlArtistSongs, id);
        }
    }
}
