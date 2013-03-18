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

    public interface IArtistsRepository : IPlaylistRepository<Artist>
    {
    }

    public class ArtistsRepository : RepositoryBase, IArtistsRepository
    {
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

        private const string SqlArtistSongs = @"
select s.*,
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
where a.ArtistId = ?1
order by a.[Year], a.TitleNorm, coalesce(nullif(s.Disc, 0), 1), s.Track
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

            return await this.Connection.QueryAsync<Artist>(sql.ToString(), string.Format("%{0}%", searchQueryNorm.Normalize()));
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
