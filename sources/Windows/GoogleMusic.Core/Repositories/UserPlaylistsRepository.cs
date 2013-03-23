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

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web;

    public interface IUserPlaylistsRepository : IPlaylistRepository<UserPlaylist>
    {
        Task<UserPlaylist> CreateAsync(string name);

        Task<IList<UserPlaylistEntry>> GetAllEntriesAsync(int sondId);

        Task<bool> DeleteAsync(UserPlaylist playlistId);

        Task<bool> ChangeName(UserPlaylist playlistId, string name);

        Task<bool> RemoveEntry(UserPlaylist playlistId, string songId, string entryId);

        Task<bool> AddEntriesAsync(UserPlaylist playlistId, List<SongBindingModel> song);
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
select s.*,
       e.[Id] as [UserPlaylistEntry.Id],
       e.[PlaylistId] as [UserPlaylistEntry.PlaylistId], 
       e.[SongId] as [UserPlaylistEntry.SongId],
       e.[PlaylistOrder] as [UserPlaylistEntry.PlaylistOrder],
       e.[ProviderEntryId] as [UserPlaylistEntry.ProviderEntryId],
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
     inner join UserPlaylistEntry e on e.SongId = s.SongId
     inner join Album a on s.[AlbumTitleNorm] = a.[TitleNorm] and coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = a.[ArtistTitleNorm]
     inner join Artist ta on ta.[TitleNorm] = a.[ArtistTitleNorm] 
where e.[PlaylistId] = ?1
order by e.[PlaylistOrder]
";

        private readonly ILogger logger;

        private readonly IPlaylistsWebService playlistsWebService;

        public UserPlaylistsRepository(
            ILogManager logManager,
            IPlaylistsWebService playlistsWebService)
        {
            this.logger = logManager.CreateLogger("userPlaylistsRepository");
            this.playlistsWebService = playlistsWebService;
        }

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
        
        public async Task<UserPlaylist> CreateAsync(string name)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Creating playlist '{0}'.", name);
            }

            var resp = await this.playlistsWebService.CreateAsync(name);
            if (resp.Success.HasValue && resp.Success.Value)
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug(
                        "Playlist was created on the server with id '{0}' for name '{1}'.", resp.Id, resp.Title);
                }

                var userPlaylistEntity = new UserPlaylist() { ProviderPlaylistId = resp.Id, Title = resp.Title, TitleNorm = resp.Title.Normalize() };
                await this.Connection.InsertAsync(userPlaylistEntity);
                return userPlaylistEntity;
            }
            else
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug(
                        "Could not create playlist for name '{0}'.", resp.Title);
                }

                return null;
            }
        }

        public async Task<IList<UserPlaylistEntry>> GetAllEntriesAsync(int sondId)
        {
            return await this.Connection.Table<UserPlaylistEntry>().Where(x => x.SongId == sondId).ToListAsync();
        }

        public async Task<bool> DeleteAsync(UserPlaylist playlist)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Deleting playlist '{0}'.", playlist.ProviderPlaylistId);
            }

            var resp = await this.playlistsWebService.DeleteAsync(playlist.ProviderPlaylistId);
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Deleting playlist '{0}'. Response '{1}'.", playlist.ProviderPlaylistId, resp);
            }

            if (resp)
            {
                await this.Connection.RunInTransactionAsync(
                    (connection) =>
                        {
                            connection.Execute("DELETE from UserPlaylistEntry where ProviderPlaylistId = ?", playlist.Id);
                            connection.Delete<UserPlaylist>(playlist.Id);
                        });
            }

            return resp;
        }

        public async Task<bool> ChangeName(UserPlaylist playlist, string name)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Changing name for playlist with Id '{0}' to '{1}'.", playlist.ProviderPlaylistId, name);
            }

            bool result = await this.playlistsWebService.ChangeNameAsync(playlist.ProviderPlaylistId, name);

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("The result of name changing for playlist with id '{0}' is '{1}'.", playlist.ProviderPlaylistId, result);
            }

            if (result)
            {
                await this.Connection.RunInTransactionAsync(
                    (connection) =>
                    {
                        connection.Execute("UPDATE UserPlaylist SET Title = ? WHERE ProviderPlaylistId = ?", name, playlist.Id);
                    });
            }

            return result;
        }

        public async Task<bool> RemoveEntry(UserPlaylist playlist, string songId, string entryId)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Removing entry Id '{0}' from playlist '{1}'.", entryId, playlist.ProviderPlaylistId);
            }

            var result = await this.playlistsWebService.RemoveSongAsync(playlist.ProviderPlaylistId, songId, entryId);
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Result of entry removing '{0}' from playlist '{1}' is '{2}'.", entryId, playlist.ProviderPlaylistId, result);
            }

            if (result)
            {
                await this.Connection.RunInTransactionAsync(connection =>
                    {
                        var entry = connection.Find<UserPlaylistEntry>(e => e.PlaylistId == playlist.Id && e.ProviderEntryId == entryId);
                        connection.Execute(
                            "UPDATE UserPlaylistEntry SET PlaylistOrder = (PlaylistOrder - 1) WHERE PlaylistId = ? AND PlaylistOrder > ?",
                            playlist.Id,
                            entry.PlaylistOrder);
                        connection.Delete(entry);
                    });
            }

            return result;
        }

        public async Task<bool> AddEntriesAsync(UserPlaylist playlist, List<SongBindingModel> songs)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Adding song Ids '{0}' to playlist '{1}'.", string.Join(",", songs.Select(x => x.Metadata.ProviderSongId.ToString())), playlist.ProviderPlaylistId);
            }

            var result = await this.playlistsWebService.AddSongAsync(playlist.ProviderPlaylistId, songs.Select(s => s.Metadata.ProviderSongId));
            if (result != null && result.SongIds.Length == 1)
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug(
                        "Successfully added entries '{0}' to playlist {1}.",
                        string.Join(",", songs.Select(x => x.Metadata.ProviderSongId.ToString())),
                        playlist.ProviderPlaylistId);
                }

                await this.Connection.RunInTransactionAsync(
                    (connection) =>
                        {
                            var lastEntry = connection.Table<UserPlaylistEntry>()
                                          .Where(e => e.Id == playlist.Id)
                                          .OrderByDescending(x => x.PlaylistOrder)
                                          .FirstOrDefault();

                            int nextIndex = lastEntry == null ? 0 : (lastEntry.PlaylistOrder + 1);

                            var entries = result.SongIds
                                            .Select((x, index) => Tuple.Create(x.PlaylistEntryId, songs.FirstOrDefault(s => string.Equals(s.Metadata.ProviderSongId, x.SongId)), index))
                                            .Where(x => x.Item2 != null)
                                            .Select(x => new UserPlaylistEntry()
                                               {
                                                   ProviderEntryId = x.Item1,
                                                   SongId = x.Item2.Metadata.SongId,
                                                   PlaylistId = playlist.Id,
                                                   PlaylistOrder = nextIndex + x.Item3
                                               });

                            connection.InsertAll(entries);
                        });

                return true;
            }

            if (this.logger.IsWarningEnabled)
            {
                this.logger.Warning(
                    "Result of adding entries '{0}' to playlist {1} was unsuccesefull.", string.Join(",", songs.Select(x => x.Metadata.ProviderSongId.ToString())), playlist.ProviderPlaylistId);
            }

            return false;
        }
    }
}