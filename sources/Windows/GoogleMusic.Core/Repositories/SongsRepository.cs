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
    using OutcoldSolutions.GoogleMusic.Services;

    public interface ISongsRepository
    {
        Task<Song> GetSongAsync(int songId);

        Task<Song> FindSongAsync(string providerSongId);

        Task<IList<Song>> SearchAsync(string searchQuery, uint? take = null);

        Task UpdateRatingAsync(Song song);

        Task UpdatePlayCountsAsync(Song song);

        Task InsertAsync(IEnumerable<Song> songs);

        Task DeleteAsync(IEnumerable<Song> songs);
    }

    public class SongsRepository : RepositoryBase, ISongsRepository
    {
        private const string SqlSearchSongs = @"
select s.* 
from [Song] as s
where (?1 = 1 or s.[IsCached] = 1) and s.[TitleNorm] like ?2
order by s.[TitleNorm]
";

        private const string SqlSong = @"
select s.* 
from [Song] as s
where (?1 = 1 or s.[IsCached] = 1) and s.[SongId] = ?2
";

        private readonly IApplicationStateService stateService;

        public SongsRepository(IApplicationStateService stateService)
        {
            this.stateService = stateService;
        }

        public Task<Song> FindSongAsync(string providerSongId)
        {
            return this.Connection.Table<Song>().Where(s => s.ProviderSongId == providerSongId).FirstOrDefaultAsync();
        }

        public async Task<IList<Song>> SearchAsync(string searchQuery, uint? take = null)
        {
            var searchQueryNorm = searchQuery.Normalize() ?? string.Empty;

            var sql = new StringBuilder(SqlSearchSongs);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Song>(sql.ToString(), this.stateService.IsOnline(), string.Format("%{0}%", searchQueryNorm));
        }

        public Task UpdateRatingAsync(Song song)
        {
            return this.Connection.ExecuteAsync("update Song set Rating = ?1 where SongId = ?2", song.Rating, song.SongId);
        }

        public Task UpdatePlayCountsAsync(Song song)
        {
            return this.Connection.ExecuteAsync("update Song set PlayCount = ?1 where SongId = ?2", song.PlayCount, song.SongId);
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
            return (await this.Connection.QueryAsync<Song>(SqlSong, this.stateService.IsOnline(), songId)).FirstOrDefault();
        }
    }
}