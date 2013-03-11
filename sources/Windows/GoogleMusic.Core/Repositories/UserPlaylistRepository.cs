// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web;

    public class UserPlaylistRepository : RepositoryBase, IUserPlaylistRepository
    {
        private readonly ILogger logger;

        private readonly IPlaylistsWebService playlistsWebService;


        public UserPlaylistRepository(
            ILogManager logManager,
            IPlaylistsWebService playlistsWebService)
        {
            this.logger = logManager.CreateLogger("UserPlaylistRepository");
            this.playlistsWebService = playlistsWebService;
        }

        public async Task<IEnumerable<UserPlaylist>> GetAllAsync()
        {
            List<UserPlaylist> userPlaylists = new List<UserPlaylist>();

            var playlists = await this.Connection.Table<UserPlaylistEntity>().ToListAsync();

            foreach (var playlist in playlists)
            {
                List<string> entrieIds = new List<string>();
                List<Song> songs = new List<Song>();

                string playlistId = playlist.Id;
                var entries = await this.Connection.Table<UserPlaylistEntryEntity>()
                                    .Where(x => x.PlaylistId == playlistId)
                                    .OrderBy(x => x.PlaylistOrder).ToListAsync();

                foreach (var entry in entries)
                {
                    var song = await this.Connection.FindAsync<SongEntity>(entry.SongId);

                    if (song != null)
                    {
                        entrieIds.Add(entry.GoogleMusicEntryId);
                        songs.Add(new Song(song));
                    }
                    else
                    {
                        this.logger.Warning("Could not find a song with id {0}.", entry.SongId);
                    }
                }

                userPlaylists.Add(new UserPlaylist(playlistId, playlist.Title, songs, entrieIds));
            }

            return userPlaylists;
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

                await this.Connection.InsertAsync(new UserPlaylistEntity() { Id = resp.Id, Title = resp.Title });
                return new UserPlaylist(resp.Id, resp.Title, new List<Song>(), new List<string>());
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

        public async Task<bool> DeleteAsync(string playlistId)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Deleting playlist '{0}'.", playlistId);
            }

            var resp = await this.playlistsWebService.DeleteAsync(playlistId);
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Deleting playlist '{0}'. Response '{1}'.", playlistId, resp);
            }

            if (resp)
            {
                await this.Connection.RunInTransactionAsync(
                    (connection) =>
                        {
                            connection.Execute("DELETE from UserPlaylistEntry where PlaylistId = ?", playlistId);
                            connection.Delete<UserPlaylistEntity>(playlistId);
                        });
            }

            return resp;
        }

        public async Task<bool> ChangeName(string playlistId, string name)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Changing name for playlist with Id '{0}' to '{1}'.", playlistId, name);
            }

            bool result = await this.playlistsWebService.ChangeNameAsync(playlistId, name);

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("The result of name changing for playlist with id '{0}' is '{1}'.", playlistId, result);
            }

            if (result)
            {
                await this.Connection.RunInTransactionAsync(
                    (connection) =>
                    {
                        connection.Execute("UPDATE UserPlaylist SET Title = ? WHERE Id = ?", name, playlistId);
                    });
            }

            return result;
        }

        public async Task<bool> RemoveEntry(string playlistId, string songId, string entryId)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Removing entry Id '{0}' from playlist '{1}'.", entryId, playlistId);
            }

            var result = await this.playlistsWebService.RemoveSongAsync(playlistId, songId, entryId);
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Result of entry removing '{0}' from playlist '{1}' is '{2}'.", entryId, playlistId, result);
            }

            if (result)
            {
                await this.Connection.RunInTransactionAsync(connection =>
                    {
                        var entry = connection.Find<UserPlaylistEntryEntity>(e => e.PlaylistId == playlistId && e.GoogleMusicEntryId == entryId);
                        connection.Execute(
                            "UPDATE UserPlaylistEntry SET PlaylistOrder = (PlaylistOrder - 1) WHERE PlaylistId = ? AND PlaylistOrder > ?",
                            playlistId,
                            entry.PlaylistOrder);
                        connection.Delete(entry);
                    });
            }

            return result;
        }

        public async Task<bool> AddEntriesAsync(string playlistId, List<Song> songs)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Adding song Ids '{0}' to playlist '{1}'.", string.Join(",", songs.Select(x => x.Metadata.Id.ToString())), playlistId);
            }

            var result = await this.playlistsWebService.AddSongAsync(playlistId, songs.Select(s => s.Metadata.Id));
            if (result != null && result.SongIds.Length == 1)
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug(
                        "Successfully added entries '{0}' to playlist {1}.",
                        string.Join(",", songs.Select(x => x.Metadata.Id.ToString())),
                        playlistId);
                }

                await this.Connection.RunInTransactionAsync(
                    (connection) =>
                        {
                            var lastEntry = connection.Table<UserPlaylistEntryEntity>()
                                          .Where(e => e.PlaylistId == playlistId)
                                          .OrderByDescending(x => x.PlaylistOrder)
                                          .FirstOrDefault();

                            int nextIndex = lastEntry == null ? 0 : (lastEntry.PlaylistOrder + 1);

                            var entries = result.SongIds.Select((x, index) => new UserPlaylistEntryEntity()
                                               {
                                                   GoogleMusicEntryId = x.PlaylistEntryId,
                                                   SongId = x.SongId,
                                                   PlaylistId = playlistId,
                                                   PlaylistOrder = nextIndex + index
                                               });

                            connection.InsertAll(entries);
                        });

                return true;
            }

            if (this.logger.IsWarningEnabled)
            {
                this.logger.Warning(
                    "Result of adding entries '{0}' to playlist {1} was unsuccesefull.", string.Join(",", songs.Select(x => x.Metadata.Id.ToString())), playlistId);
            }

            return false;
        }
    }
}