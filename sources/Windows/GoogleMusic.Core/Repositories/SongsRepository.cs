// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface ISongsRepository
    {
        Task<Song> GetSongAsync(int songId);

        Task<IList<Song>> SearchAsync(string searchQuery, uint? take = null);

        Task UpdateAsync(IEnumerable<Song> songs);

        Task InsertAsync(IEnumerable<Song> songs);

        Task DeleteAsync(IEnumerable<Song> songs);
    }

    public class SongsRepository : RepositoryBase, ISongsRepository
    {
        private const string SqlSearchSongs = @"
select s.* 
from [Song] as s
where s.[TitleNorm] like ?1
order by s.[TitleNorm]
";

        private const string SqlSong = @"
select s.* 
from [Song] as s
where s.[SongId] = ?1
";

        public async Task<IList<Song>> SearchAsync(string searchQuery, uint? take = null)
        {
            var searchQueryNorm = searchQuery.Normalize() ?? string.Empty;

            var sql = new StringBuilder(SqlSearchSongs);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Song>(sql.ToString(), string.Format("%{0}%", searchQueryNorm));
        }

        public Task UpdateAsync(IEnumerable<Song> songs)
        {
            return this.Connection.RunInTransactionAsync((c) => c.UpdateAll(songs));
        }

        public Task InsertAsync(IEnumerable<Song> songs)
        {
            return this.Connection.RunInTransactionAsync((c) => c.InsertAll(songs));
        }

        public Task DeleteAsync(IEnumerable<Song> songs)
        {
            return this.Connection.RunInTransactionAsync((c) =>
                {
                    foreach (var song in songs)
                    {
                        c.Delete(song);
                    }
                });
        }

        public async Task<Song> GetSongAsync(int songId)
        {
            return (await this.Connection.QueryAsync<Song>(SqlSong, songId)).FirstOrDefault();
        }
    }
}