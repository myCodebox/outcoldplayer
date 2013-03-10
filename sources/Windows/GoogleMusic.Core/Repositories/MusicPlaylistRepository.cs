// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web;

    using SQLite;

    public class MusicPlaylistRepository : RepositoryBase, IMusicPlaylistRepository
    {
        private readonly ILogger logger;

        private readonly IPlaylistsWebService playlistsWebService;


        public MusicPlaylistRepository(
            ILogManager logManager,
            IPlaylistsWebService playlistsWebService)
        {
            this.logger = logManager.CreateLogger("MusicPlaylistRepository");
            this.playlistsWebService = playlistsWebService;
        }

        public async Task<IEnumerable<MusicPlaylist>> GetAllAsync()
        {
            List<MusicPlaylist> userPlaylists = new List<MusicPlaylist>();

            var playlists = await this.Connection.Table<UserPlaylistEntity>().ToListAsync();

            foreach (var playlist in playlists)
            {
                List<string> entrieIds = new List<string>();
                List<Song> songs = new List<Song>();

                string playlistId = playlist.Id;
                var entries = await this.Connection.Table<UserPlaylistEntryEntity>().Where(x => x.PlaylistId == playlistId).ToListAsync();
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

                userPlaylists.Add(new MusicPlaylist(playlistId, playlist.Title, songs, entrieIds));
            }

            return userPlaylists;
        }

        public async Task<MusicPlaylist> CreateAsync(string name)
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
                return new MusicPlaylist(resp.Id, resp.Title, new List<Song>(), new List<string>());
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
                    (SQLiteConnection connection) =>
                        {
                            connection.Execute("delete from UserPlaylist where Id = ?", playlistId);
                            connection.Execute("delete from UserPlaylistEntry where PlaylistId = ?", playlistId);
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
                this.logger.Debug("The result of name changing for playlist with id '{0}' is '{1}'.", result);
            }

            if (result)
            {
                await this.Connection.RunInTransactionAsync(
                    (SQLiteConnection connection) =>
                    {
                        connection.Execute("update UserPlaylist set Title = ? where Id = ?", name, playlistId);
                    });
            }

            return result;
        }

        public async Task<bool> RemoveEntry(string playlistId, string entryId)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Removing entry Id '{0}' from playlist '{1}'.", entryId, playlistId);
            }

            //MusicPlaylist musicPlaylist;
            //if (!this.musicPlaylists.TryGetValue(playlistId, out musicPlaylist))
            //{
            //    this.logger.Warning("Cannot find playlist with id '{0}', could not delete entry {1}.", playlistId, entryId);
            //    return false;
            //}

            //var index = musicPlaylist.EntriesIds.IndexOf(entryId);
            //var song = musicPlaylist.Songs[index];

            //var result = await this.playlistsWebService.RemoveSongAsync(playlistId, song.Metadata.Id, entryId);

            //if (this.logger.IsDebugEnabled)
            //{
            //    this.logger.Debug("Result of entry removing '{0}' from playlist '{1}' is '{2}'.", entryId, playlistId, result);
            //}

            //if (result)
            //{
            //    musicPlaylist.EntriesIds.RemoveAt(index);
            //    musicPlaylist.Songs.RemoveAt(index);
            //    musicPlaylist.CalculateFields();
            //}

            return true;
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

            //MusicPlaylist musicPlaylist;
            //if (!this.musicPlaylists.TryGetValue(playlistId, out musicPlaylist))
            //{
            //    this.logger.Warning("Cannot find playlist with id '{0}', could not add entries.", playlistId);
            //    return false;
            //}

            //var result = await this.playlistsWebService.AddSongAsync(playlistId, songs.Select(s => s.Metadata.Id));
            //if (result != null && result.SongIds.Length == 1)
            //{
            //    if (this.logger.IsDebugEnabled)
            //    {
            //        this.logger.Debug(
            //            "Successfully added entries '{0}' to playlist {1}.",
            //            string.Join(",", songs.Select(x => x.Metadata.Id.ToString())),
            //            playlistId);
            //    }

            //    foreach (var songIdResp in result.SongIds)
            //    {
            //        Song song = songs.FirstOrDefault(x => string.Equals(x.Metadata.Id, songIdResp.SongId));
                    
            //        if (song != null)
            //        {
            //            musicPlaylist.Songs.Add(song);
            //            musicPlaylist.EntriesIds.Add(songIdResp.PlaylistEntryId);
            //        }
            //        else
            //        {
            //            this.logger.Warning("Could not find song with Id '{0}'.", songIdResp.SongId);
            //        }
            //    }

            //    musicPlaylist.CalculateFields();

            //    return true;
            //}

            if (this.logger.IsWarningEnabled)
            {
                this.logger.Warning(
                    "Result of adding entries '{0}' to playlist {1} was unsuccesefull.", string.Join(",", songs.Select(x => x.Metadata.Id.ToString())), playlistId);
            }

            return false;
        }
    }
}