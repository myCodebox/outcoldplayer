// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    using Windows.UI.Xaml;

    public class MusicPlaylistRepository : IMusicPlaylistRepository
    {
        private readonly ILogger logger;

        private readonly ConcurrentDictionary<string, MusicPlaylist> musicPlaylists = new ConcurrentDictionary<string, MusicPlaylist>();
        
        private readonly IPlaylistsWebService playlistsWebService;
        private readonly ISongsRepository songsRepository;

        private DispatcherTimer dispatcherTimer;

        public MusicPlaylistRepository(
            ILogManager logManager,
            IPlaylistsWebService playlistsWebService,
            ISongsRepository songsRepository,
            IGoogleMusicSessionService googleMusicSessionService)
        {
            this.logger = logManager.CreateLogger("MusicPlaylistRepository");
            this.playlistsWebService = playlistsWebService;
            this.songsRepository = songsRepository;

            googleMusicSessionService.SessionCleared += (sender, args) =>
                {
                    this.logger.Debug("Session cleared. Stopping the dispatcher and clearing the cache of playlists.");

                    this.dispatcherTimer.Stop();
                    this.dispatcherTimer = null;
                    this.musicPlaylists.Clear();
                };
        }

        public async Task InitializeAsync()
        {
            this.logger.Debug("Initializing.");

            await this.UpdatePlaylistsAsync();

            this.logger.Debug("Initialized. Creating dispatcher timer.");

            this.dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(10) };
#if NETFX_CORE
            this.dispatcherTimer.Tick += async (sender, o) => await this.UpdatePlaylistsAsync();
#endif
            this.dispatcherTimer.Start();
        }

        public Task<IEnumerable<MusicPlaylist>> GetAllAsync()
        {
            return Task.FromResult(this.musicPlaylists.Values.ToList().AsEnumerable());
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

                var playlist = new MusicPlaylist(resp.Id, resp.Title, new List<Song>(), new List<string>());
                this.musicPlaylists.AddOrUpdate(resp.Id, guid => playlist, (guid, musicPlaylist) => playlist);
                return playlist;
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
                MusicPlaylist musicPlaylist;
                if (!this.musicPlaylists.TryRemove(playlistId, out musicPlaylist))
                {
                    if (this.logger.IsWarningEnabled)
                    {
                        this.logger.Debug("Could not remove playlist '{0}' from collection.");
                    }
                }
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
                MusicPlaylist musicPlaylist;
                if (this.musicPlaylists.TryGetValue(playlistId, out musicPlaylist))
                {
                    musicPlaylist.Title = name;
                }
                else
                {
                    this.logger.Warning("After renaming we could not find playlist with id '{0}' in music playlist collection.", playlistId);
                }
            }

            return result;
        }

        public async Task<bool> RemoveEntry(string playlistId, string entryId)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Removing entry Id '{0}' from playlist '{1}'.", entryId, playlistId);
            }

            MusicPlaylist musicPlaylist;
            if (!this.musicPlaylists.TryGetValue(playlistId, out musicPlaylist))
            {
                this.logger.Warning("Cannot find playlist with id '{0}', could not delete entry {1}.", playlistId, entryId);
                return false;
            }

            var index = musicPlaylist.EntriesIds.IndexOf(entryId);
            var song = musicPlaylist.Songs[index];

            var result = await this.playlistsWebService.RemoveSongAsync(playlistId, song.Metadata.Id, entryId);

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Result of entry removing '{0}' from playlist '{1}' is '{2}'.", entryId, playlistId, result);
            }

            if (result)
            {
                musicPlaylist.EntriesIds.RemoveAt(index);
                musicPlaylist.Songs.RemoveAt(index);
                musicPlaylist.CalculateFields();
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

            MusicPlaylist musicPlaylist;
            if (!this.musicPlaylists.TryGetValue(playlistId, out musicPlaylist))
            {
                this.logger.Warning("Cannot find playlist with id '{0}', could not add entries.", playlistId);
                return false;
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

                foreach (var songIdResp in result.SongIds)
                {
                    Song song = songs.FirstOrDefault(x => string.Equals(x.Metadata.Id, songIdResp.SongId));
                    
                    if (song != null)
                    {
                        musicPlaylist.Songs.Add(song);
                        musicPlaylist.EntriesIds.Add(songIdResp.PlaylistEntryId);
                    }
                    else
                    {
                        this.logger.Warning("Could not find song with Id '{0}'.", songIdResp.SongId);
                    }
                }

                musicPlaylist.CalculateFields();

                return true;
            }

            if (this.logger.IsWarningEnabled)
            {
                this.logger.Warning(
                    "Result of adding entries '{0}' to playlist {1} was unsuccesefull.", string.Join(",", songs.Select(x => x.Metadata.Id.ToString())), playlistId);
            }

            return false;
        }

        public void ClearRepository()
        {
            this.dispatcherTimer.Stop();
            this.dispatcherTimer = null;
            this.musicPlaylists.Clear();
        }

        private async Task UpdatePlaylistsAsync()
        {
            this.logger.Debug("Updating playlists.");

            var googlePlaylists = await this.playlistsWebService.GetAllAsync();
            var existingPlaylistIds = this.musicPlaylists.Keys.ToList();

            if (googlePlaylists.Playlists != null)
            {
                foreach (var googlePlaylist in googlePlaylists.Playlists)
                {
                    var playlistId = googlePlaylist.PlaylistId;
                    bool addPlaylist = false;

                    existingPlaylistIds.Remove(playlistId);

                    MusicPlaylist playlist;
                    if (this.musicPlaylists.TryGetValue(playlistId, out playlist))
                    {
                        if (!this.AreSame(playlist, googlePlaylist))
                        {
                            if (this.logger.IsDebugEnabled)
                            {
                                this.logger.Debug("Playlist '{0}' are not the same. Removing it to create new.", playlistId);
                            }

                            this.musicPlaylists.TryRemove(playlistId, out playlist);
                            addPlaylist = true;
                        }
                    }
                    else
                    {
                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug("New playlist loaded '{0}'.", playlistId);
                        }

                        addPlaylist = true;
                    }
                    
                    if (addPlaylist)
                    {
                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug("Creating new MusicPlaylist instance with id '{0}'.", playlistId);
                        }

                        Dictionary<string, Song> playlistSongs = new Dictionary<string, Song>();

                        if (googlePlaylist.Playlist != null)
                        {
                            foreach (var s in googlePlaylist.Playlist)
                            {
                                var song = await this.songsRepository.GetSongAsync(s.Id);

                                if (s.PlaylistEntryId != null && song != null)
                                {
                                    playlistSongs.Add(s.PlaylistEntryId, song);
                                }
                            }
                        }

                        if (googlePlaylist.Playlist != null && playlistSongs.Count != googlePlaylist.Playlist.Count)
                        {
                            this.logger.Warning(
                                "We could not get all songs for playlist '{0}' from repository. Playlist count: {1}. Loaded songs: {2}..",
                                playlistId,
                                googlePlaylist.Playlist.Count,
                                playlistSongs.Count);
                        }

                        playlist = new MusicPlaylist(playlistId, googlePlaylist.Title, playlistSongs.Values.ToList(), playlistSongs.Keys.ToList());
                        this.musicPlaylists.AddOrUpdate(playlistId, guid => playlist, (guid, musicPlaylist) => playlist);
                    }
                }
            }

            foreach (var playlistId in existingPlaylistIds)
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("We did not get playlist with id '{0}'. Removing it.", playlistId);
                }

                MusicPlaylist musicPlaylist;
                this.musicPlaylists.TryRemove(playlistId, out musicPlaylist);
            }

            this.logger.Debug("Playlists are updated.");
        }

        private bool AreSame(MusicPlaylist musicPlaylist, GoogleMusicPlaylist googleMusicPlaylist)
        {
            if (!string.Equals(musicPlaylist.Title, googleMusicPlaylist.Title, StringComparison.CurrentCulture))
            {
                return false;
            }

            int countOfSongs = googleMusicPlaylist.Playlist == null ? 0 : googleMusicPlaylist.Playlist.Count;
            if (countOfSongs != musicPlaylist.Songs.Count)
            {
                return false;
            }
            
            Debug.Assert(googleMusicPlaylist.Playlist != null, "googleMusicPlaylist.Playlist != null");
            for (int i = 0; i < countOfSongs; i++)
            {
                var googlePlaylistSong = googleMusicPlaylist.Playlist[i];

                if (googlePlaylistSong.Id != musicPlaylist.Songs[i].Metadata.Id
                    || !string.Equals(googlePlaylistSong.PlaylistEntryId, musicPlaylist.EntriesIds[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}