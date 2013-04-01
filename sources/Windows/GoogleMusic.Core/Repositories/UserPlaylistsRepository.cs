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

    public interface IUserPlaylistsRepository : IPlaylistRepository<UserPlaylist>
    {
        Task InstertAsync(UserPlaylist userPlaylist);

        Task DeleteAsync(UserPlaylist userPlaylist);

        Task UpdateAsync(UserPlaylist userPlaylist);

        Task<IList<UserPlaylistEntry>> GetAllSongEntriesAsync(int sondId);

        Task DeleteEntriesAsync(IEnumerable<UserPlaylistEntry> entries);

        Task InsertEntriesAsync(IEnumerable<UserPlaylistEntry> entries);

        Task UpdateEntriesAsync(IEnumerable<UserPlaylistEntry> entries);
    }

    public class UserPlaylistsRepository : RepositoryBase, IUserPlaylistsRepository
    {
        private const string SqlSearchPlaylists = @"
select p.*
from [UserPlaylist] as p  
where p.[TitleNorm] like ?1
order by p.[TitleNorm]
";

        private const string SqlUserPlaylistSongs = @"
select s.*
from [Song] as s
     inner join UserPlaylistEntry e on e.SongId = s.SongId
where e.[PlaylistId] = ?1
order by e.[PlaylistOrder]
";

        private const string SqlDeletePlaylistEntries = @"
DELETE FROM [UserPlaylistEntry] WHERE PlaylistId = ?1
";

        public async Task<int> GetCountAsync()
        {
            return await this.Connection.Table<UserPlaylist>().CountAsync();
        }

        public async Task<IList<UserPlaylist>> GetAllAsync(Order order, uint? take = null)
        {
            var query = this.Connection.Table<UserPlaylist>(); 

            if (order == Order.Name)
            {
                query = query.OrderBy(p => p.TitleNorm);
            }
            else if (order == Order.LastPlayed)
            {
                query = query.OrderByDescending(p => p.LastPlayed);
            }

            if (take.HasValue)
            {
                query = query.Take((int)take.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<UserPlaylist> GetAsync(int id)
        {
            return await this.Connection.Table<UserPlaylist>().Where(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IList<Song>> GetSongsAsync(int id)
        {
            return await this.Connection.QueryAsync<Song>(SqlUserPlaylistSongs, id);
        }

        public async Task<IList<UserPlaylist>> SearchAsync(string searchQuery, uint? take)
        {
            var searchQueryNorm = searchQuery.Normalize() ?? string.Empty;

            var sql = new StringBuilder(SqlSearchPlaylists);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<UserPlaylist>(sql.ToString(), string.Format("%{0}%", searchQueryNorm));
        }
        
        public Task InstertAsync(UserPlaylist userPlaylist)
        {
            return this.Connection.InsertAsync(userPlaylist);
        }

        public async Task<IList<UserPlaylistEntry>> GetAllSongEntriesAsync(int sondId)
        {
            return await this.Connection.Table<UserPlaylistEntry>().Where(x => x.SongId == sondId).ToListAsync();
        }

        public Task DeleteAsync(UserPlaylist playlist)
        {
            return this.Connection.RunInTransactionAsync(
                    (connection) =>
                    {
                        connection.Execute(SqlDeletePlaylistEntries, playlist.Id);
                        connection.Delete<UserPlaylist>(playlist.Id);
                    });
        }

        public Task UpdateAsync(UserPlaylist playlist)
        {
            return this.Connection.UpdateAsync(playlist);
        }

        public Task DeleteEntriesAsync(IEnumerable<UserPlaylistEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException("entries");
            }

            return this.Connection.RunInTransactionAsync(connection =>
            {
                foreach (var userPlaylistEntry in entries)
                {
                    connection.Delete(userPlaylistEntry);
                }
            });
        }

        public Task InsertEntriesAsync(IEnumerable<UserPlaylistEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException("entries");
            }

            return this.Connection.InsertAllAsync(entries);
        }

        public Task UpdateEntriesAsync(IEnumerable<UserPlaylistEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException("entries");
            }

            return this.Connection.RunInTransactionAsync(connection =>
            {
                foreach (var userPlaylistEntry in entries)
                {
                    connection.Update(userPlaylistEntry);
                }
            });
        }
    }
}