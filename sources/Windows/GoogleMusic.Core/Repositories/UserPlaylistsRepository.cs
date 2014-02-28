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

    public interface IUserPlaylistsRepository : IPlaylistRepository<UserPlaylist>
    {
        Task InsertAsync(IEnumerable<UserPlaylist> userPlaylist);

        Task DeleteAsync(IEnumerable<UserPlaylist> userPlaylist);

        Task UpdateAsync(IEnumerable<UserPlaylist> userPlaylist);

        Task<IList<UserPlaylistEntry>> GetAllSongEntriesAsync(string sondId);

        Task DeleteEntriesAsync(IEnumerable<UserPlaylistEntry> entries);

        Task InsertEntriesAsync(IEnumerable<UserPlaylistEntry> entries);

        Task UpdateEntriesAsync(IEnumerable<UserPlaylistEntry> entries);

        Task<UserPlaylist> FindUserPlaylistAsync(Song song);

        Task<UserPlaylistEntry> GetEntryAsync(string id);
    }

    public class UserPlaylistsRepository : RepositoryBase, IUserPlaylistsRepository
    {
        private const string SqlSearchPlaylists = @"
select p.*
from [UserPlaylist] as p  
where (?1 = 1 or p.[OfflineSongsCount] > 0) and p.[TitleNorm] like ?2
order by p.[TitleNorm]
";

        private const string SqlUserPlaylistSongs = @"
select s.*,
    e.[Id] as [UserPlaylistEntry.Id],
    e.[PlaylistId] as [UserPlaylistEntry.PlaylistId], 
    e.[SongId] as [UserPlaylistEntry.SongId],
    e.[PlaylistOrder] as [UserPlaylistEntry.PlaylistOrder],
    e.[CreationDate] as [UserPlaylistEntry.CreationDate],
    e.[LastModified] as [UserPlaylistEntry.LastModified],
    e.[CliendId] as [UserPlaylistEntry.CliendId],
    e.[Source] as [UserPlaylistEntry.Source]
from [Song] as s
     inner join UserPlaylistEntry e on e.SongId = s.SongId
where (?1 = 1 or s.IsCached = 1) and e.[PlaylistId] = ?2
order by e.[PlaylistOrder]
";

        private const string SqlDeletePlaylistEntries = @"
DELETE FROM [UserPlaylistEntry] WHERE PlaylistId = ?1
";

        private const string SqlFindFirstUserPlaylist = @"
select p.* 
from UserPlaylist p
where exists(select * from UserPlaylistEntry e where e.PlaylistId = p.PlaylistId and e.SongId = ?1)
limit 1
";

        private readonly IApplicationStateService stateService;

        public UserPlaylistsRepository(IApplicationStateService stateService)
        {
            this.stateService = stateService;
        }

        public async Task<int> GetCountAsync()
        {
            var query = this.Connection.Table<UserPlaylist>();

            if (this.stateService.IsOffline())
            {
                query = query.Where(a => a.OfflineSongsCount > 0);
            }

            return await query.CountAsync();
        }

        public async Task<IList<UserPlaylist>> GetAllAsync(Order order, uint? take = null)
        {
            var query = this.Connection.Table<UserPlaylist>();

            if (this.stateService.IsOffline())
            {
                query = query.Where(a => a.OfflineSongsCount > 0);
            }

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

        public async Task<UserPlaylist> GetAsync(string id)
        {
            var query = this.Connection.Table<UserPlaylist>().Where(a => a.PlaylistId == id);

            if (this.stateService.IsOffline())
            {
                query = query.Where(a => a.OfflineSongsCount > 0);
            }

            return await query.FirstOrDefaultAsync();
        }

        public Task<UserPlaylistEntry> GetEntryAsync(string id)
        {
            return this.Connection.Table<UserPlaylistEntry>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IList<Song>> GetSongsAsync(string id, bool includeAll = false)
        {
            return await this.Connection.QueryAsync<Song>(SqlUserPlaylistSongs, includeAll || this.stateService.IsOnline(), id);
        }

        public async Task<IList<UserPlaylist>> SearchAsync(string searchQuery, uint? take)
        {
            var searchQueryNorm = searchQuery.Normalize() ?? string.Empty;

            var sql = new StringBuilder(SqlSearchPlaylists);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<UserPlaylist>(sql.ToString(), this.stateService.IsOnline(), string.Format("%{0}%", searchQueryNorm));
        }
        
        public Task InsertAsync(IEnumerable<UserPlaylist> userPlaylist)
        {
            return this.Connection.InsertAllAsync(userPlaylist);
        }

        public async Task<IList<UserPlaylistEntry>> GetAllSongEntriesAsync(string sondId)
        {
            return await this.Connection.Table<UserPlaylistEntry>().Where(x => x.SongId == sondId).ToListAsync();
        }

        public Task DeleteAsync(IEnumerable<UserPlaylist> playlists)
        {
            return this.Connection.RunInTransactionAsync(
                    (connection) =>
                    {
                        foreach (var userPlaylist in playlists)
                        {
                            connection.Execute(SqlDeletePlaylistEntries, userPlaylist.Id);
                            connection.Delete<UserPlaylist>(userPlaylist.Id);
                        }
                    });
        }

        public Task UpdateAsync(IEnumerable<UserPlaylist> playlists)
        {
            return this.Connection.RunInTransactionAsync(connection =>
            {
                foreach (var playlist in playlists)
                {
                    connection.Update(playlist);
                }
            });
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

        public async Task<UserPlaylist> FindUserPlaylistAsync(Song song)
        {
            if (song == null)
            {
                throw new ArgumentNullException("song");
            }

            return (await this.Connection.QueryAsync<UserPlaylist>(SqlFindFirstUserPlaylist, song.SongId)).FirstOrDefault();
        }
    }
}