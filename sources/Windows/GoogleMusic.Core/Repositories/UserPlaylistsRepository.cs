// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IUserPlaylistsRepository : IPlaylistRepository<UserPlaylist>
    {
        Task<int> InsertAsync(IEnumerable<UserPlaylist> userPlaylist);

        Task<int> DeleteAsync(IEnumerable<UserPlaylist> userPlaylist);

        Task<int> UpdateAsync(IEnumerable<UserPlaylist> userPlaylist);

        Task<IList<UserPlaylistEntry>> GetAllSongEntriesAsync(string sondId);

        Task<int> DeleteEntriesAsync(IEnumerable<UserPlaylistEntry> entries);

        Task<int> InsertEntriesAsync(IEnumerable<UserPlaylistEntry> entries);

        Task<int> UpdateEntriesAsync(IEnumerable<UserPlaylistEntry> entries);

        Task<UserPlaylist> FindUserPlaylistAsync(Song song);

        Task<UserPlaylistEntry> GetEntryAsync(string id);

        Task<List<UserPlaylist>> GetAllUserPlaylistsAsync();

        Task<Uri[]> GetUrisAsync(string id);
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

        private const string SqlSelectAll = @"
select p.*
from UserPlaylist p
";

        private const string SqlGetUris = @"
select *
from
(
 select distinct(s.AlbumArtUrl) as Url
 from Song s 
    inner join UserPlaylistEntry e on s.SongId = e.SongId and e.PlaylistId = ?2
 where s.AlbumArtUrl is not null and (?1 = 1 or s.[IsCached] = 1) 
 order by s.Recent desc
)
limit 4
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
            StringBuilder query = new StringBuilder(SqlSelectAll);

            if (this.stateService.IsOffline())
            {
                query.Append(" where p.OfflineSongsCount > 0 and p.Type == \"USER_GENERATED\"");
            }

            if (order == Order.Name)
            {
                query = query.Append(" order by p.TitleNorm");
            }
            else if (order == Order.LastPlayed)
            {
                query = query.Append(" order by max(p.Recent, p.CreationDate)");
            }

            if (take.HasValue)
            {
                query = query.AppendFormat(CultureInfo.InvariantCulture, " limit {0}", take.Value);
            }

            return await Connection.QueryAsync<UserPlaylist>(query.ToString(), this.stateService.IsOnline());
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
        
        public Task<int> InsertAsync(IEnumerable<UserPlaylist> userPlaylist)
        {
            return this.Connection.InsertAllAsync(userPlaylist);
        }

        public async Task<IList<UserPlaylistEntry>> GetAllSongEntriesAsync(string sondId)
        {
            return await this.Connection.Table<UserPlaylistEntry>().Where(x => x.SongId == sondId).ToListAsync();
        }

        public async Task<int> DeleteAsync(IEnumerable<UserPlaylist> playlists)
        {
            int deletedCount = 0;
            await this.Connection.RunInTransactionAsync(
                    (connection) =>
                    {
                        foreach (var userPlaylist in playlists)
                        {
                            connection.Execute(SqlDeletePlaylistEntries, userPlaylist.Id);
                            deletedCount += connection.Delete<UserPlaylist>(userPlaylist.Id);
                        }
                    });
            return deletedCount;
        }

        public async Task<int> UpdateAsync(IEnumerable<UserPlaylist> playlists)
        {
            int updatedCount = 0;
            await this.Connection.RunInTransactionAsync(connection =>
            {
                foreach (var playlist in playlists)
                {
                    updatedCount += connection.Update(playlist);
                }
            });
            return updatedCount;
        }

        public async Task<int> DeleteEntriesAsync(IEnumerable<UserPlaylistEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException("entries");
            }

            int deletedCount = 0;

            await this.Connection.RunInTransactionAsync(connection =>
            {
                foreach (var userPlaylistEntry in entries)
                {
                    deletedCount += connection.Delete(userPlaylistEntry);
                }
            });

            return deletedCount;
        }

        public Task<int> InsertEntriesAsync(IEnumerable<UserPlaylistEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException("entries");
            }

            return this.Connection.InsertAllAsync(entries);
        }

        public async Task<int> UpdateEntriesAsync(IEnumerable<UserPlaylistEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException("entries");
            }

            int updatedCount = 0;

            await this.Connection.RunInTransactionAsync(connection =>
            {
                foreach (var userPlaylistEntry in entries)
                {
                    updatedCount += connection.Update(userPlaylistEntry);
                }
            });

            return updatedCount;
        }

        public async Task<UserPlaylist> FindUserPlaylistAsync(Song song)
        {
            if (song == null)
            {
                throw new ArgumentNullException("song");
            }

            return (await this.Connection.QueryAsync<UserPlaylist>(SqlFindFirstUserPlaylist, song.SongId)).FirstOrDefault();
        }


        public Task<List<UserPlaylist>> GetAllUserPlaylistsAsync()
        {
            return this.Connection.Table<UserPlaylist>().Where(a => a.Type == "USER_GENERATED").OrderBy(x => x.TitleNorm).ToListAsync();
        }

        public async Task<Uri[]> GetUrisAsync(string id)
        {
             return (await this.Connection.QueryAsync<UrlRef>(SqlGetUris, this.stateService.IsOnline(), id)).Select(x => new Uri(x.Url)).ToArray();
        }
    }
}