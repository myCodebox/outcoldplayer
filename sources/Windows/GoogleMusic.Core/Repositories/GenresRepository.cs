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
select s.* 
from [Song] as s
     inner join Genre g on s.GenreTitleNorm = g.TitleNorm
where g.GenreId = ?1
order by coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]), s.AlbumTitleNorm, coalesce(nullif(s.Disc, 0), 1), s.Track
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
