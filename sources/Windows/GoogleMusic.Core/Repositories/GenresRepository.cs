// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IGenresRepository : IPlaylistRepository<Genre>
    {
    }

    public class GenresRepository : RepositoryBase, IGenresRepository
    {
        private const string SqlSearchGenres = @"
select x.*
from [Genre] as x  
where x.[TitleNorm] like ?1
order by x.[TitleNorm]
";

        private const string SqlGenreSongs = @"
select s.* ,
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
       ta.[LastPlayed]  as [Artist.LastPlayed]
from [Song] as s
     inner join Genre g on s.GenreTitleNorm = g.TitleNorm
     inner join Album a on s.[AlbumTitleNorm] = a.[TitleNorm] and coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = a.[ArtistTitleNorm]
     inner join Artist ta on ta.[TitleNorm] = a.[ArtistTitleNorm] 
where g.GenreId = ?1
order by a.ArtistTitleNorm, a.TitleNorm, coalesce(nullif(s.Disc, 0), 1), s.Track
";

        public async Task<int> GetCountAsync()
        {
            return await this.Connection.Table<Genre>().CountAsync();
        }

        public async Task<IList<Genre>> GetAllAsync(Order order, uint? take = null)
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

            return await this.Connection.QueryAsync<Genre>(sql.ToString(), string.Format("%{0}%", searchQueryNorm));
        }

        public async Task<Genre> GetAsync(int id)
        {
            return await this.Connection.Table<Genre>().Where(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IList<Song>> GetSongsAsync(int id)
        {
            return await this.Connection.QueryAsync<Song>(SqlGenreSongs, id);
        }
    }
}
