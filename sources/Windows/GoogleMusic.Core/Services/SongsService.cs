// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    using Windows.UI.Xaml;

    public class SongsService : ISongsService
    {
        private const int HighlyRatedValue = 4;

        private readonly object lockerTasks = new object();

        private readonly Dictionary<Guid, MusicPlaylist> playlistsRepository = new Dictionary<Guid, MusicPlaylist>();

        private readonly IPlaylistsWebService webService;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly ISongsRepository songsRepository;
        private readonly ISongWebService songWebService;

        private Task<List<Song>> taskAllSongsLoader = null;
        private Task<List<MusicPlaylist>> taskAllPlaylistsLoader = null;

        private DispatcherTimer timer;

        private DateTime? lastPlaylistsLoad;

        public SongsService(
            IPlaylistsWebService webService,
            IGoogleMusicSessionService sessionService,
            ISongWebService songWebService,
            ISongsRepository songsRepository)
        {
            this.webService = webService;
            this.sessionService = sessionService;
            this.songWebService = songWebService;
            this.songsRepository = songsRepository;

            this.sessionService.SessionCleared += (sender, args) =>
                {
                    this.timer.Stop();
                    this.timer = null;

                    lock (this.lockerTasks)
                    {
                        this.taskAllPlaylistsLoader = null;
                        this.taskAllSongsLoader = null;
                    }

                    this.songsRepository.Clear();
                };
        }

        public async Task<List<MusicPlaylist>> GetAllPlaylistsAsync(Order order = Order.Name, bool canReload = false)
        {
            return OrderCollection(await this.GetAllGooglePlaylistsAsync(canReload), order).ToList();
        }

        public async Task<MusicPlaylist> CreatePlaylistAsync()
        {
            var name = string.Format(CultureInfo.CurrentCulture, "Playlist - {0}", DateTime.Now);
            var resp = await this.webService.CreateAsync(name);
            if (resp != null)
            {
                this.lastPlaylistsLoad = null;
                await this.GetAllGooglePlaylistsAsync(canReload: true);

                var musicPlaylist = new MusicPlaylist(resp.Id, resp.Title, new List<Song>(), new List<Guid>());
                return musicPlaylist;
            }

            return null;
        }

        public async Task<List<SystemPlaylist>> GetSystemPlaylists()
        {
            var allSongs = await this.GetAllGoogleSongsAsync();

            SystemPlaylist allSongsPlaylist = new SystemPlaylist("All songs", SystemPlaylist.SystemPlaylistType.AllSongs,  allSongs);
            SystemPlaylist highlyRatedPlaylist = new SystemPlaylist("Highly rated", SystemPlaylist.SystemPlaylistType.HighlyRated, allSongs.Where(x => x.Rating >= HighlyRatedValue));

            return new List<SystemPlaylist>() { allSongsPlaylist, highlyRatedPlaylist };
        }

        public async Task<bool> DeletePlaylistAsync(MusicPlaylist playlist)
        {
            bool result = await this.webService.DeleteAsync(playlist.Id);

            if (result)
            {
                this.lastPlaylistsLoad = null;
                await this.GetAllGooglePlaylistsAsync(canReload: true);
            }

            return result;
        }

        public async Task<bool> ChangePlaylistNameAsync(MusicPlaylist playlist, string newName)
        {
            var result = await this.webService.ChangeNameAsync(playlist.Id, newName);

            if (result)
            {
                playlist.Title = newName;
            }

            return result;
        }

        public async Task<bool> RemoveSongFromPlaylistAsync(MusicPlaylist playlist, int index)
        {
            bool result = await this.webService.RemoveSongAsync(playlist.Id, playlist.Songs[index].GoogleMusicMetadata.Id, playlist.EntriesIds[index]);
            if (result)
            {
                playlist.EntriesIds.RemoveAt(index);
                playlist.Songs.RemoveAt(index);
                playlist.CalculateFields();
            }

            return result;
        }

        public async Task<bool> AddSongToPlaylistAsync(MusicPlaylist playlist, Song song)
        {
            var result = await this.webService.AddSongAsync(playlist.Id, song.GoogleMusicMetadata.Id);
            if (result != null && result.SongIds.Length == 1)
            {
                playlist.Songs.Add(song);
                playlist.EntriesIds.Add(result.SongIds[0].PlaylistEntryId);
                playlist.CalculateFields();
            }

            return result != null;
        }

        public async Task<List<Song>> GetAllGoogleSongsAsync(IProgress<int> progress = null)
        {
            lock (this.lockerTasks)
            {
                if (this.taskAllSongsLoader == null)
                {
                    this.taskAllSongsLoader = this.GetAllSongsTask(progress);
                }
            }

            return await this.taskAllSongsLoader;
        }

        private IEnumerable<TPlaylist> OrderCollection<TPlaylist>(IEnumerable<TPlaylist> playlists, Order order)
            where TPlaylist : Playlist
        {
            if (order == Order.LastPlayed)
            {
                playlists = playlists.OrderByDescending(x => x.Songs.Count > 0 ? x.Songs.Max(s => s.GoogleMusicMetadata.LastPlayed) : double.MinValue);
            }
            else if (order == Order.Name)
            {
                playlists = playlists.OrderBy(x => (x.Title ?? string.Empty).ToUpper());
            }

            return playlists;
        }

        private async Task<List<MusicPlaylist>> GetAllGooglePlaylistsAsync(bool canReload)
        {
            lock (this.lockerTasks)
            {
                if (this.taskAllPlaylistsLoader == null || (canReload && (this.lastPlaylistsLoad == null || (DateTime.Now - this.lastPlaylistsLoad.Value).TotalMinutes > 10)))
                {
                    this.taskAllPlaylistsLoader = this.GetAllPlaylistsTask();
                    this.lastPlaylistsLoad = DateTime.Now;
                }
            }
            
            return await this.taskAllPlaylistsLoader;
        }

        private async Task<List<MusicPlaylist>> GetAllPlaylistsTask()
        {
            var googleMusicPlaylists = await this.webService.GetAllAsync();

            List<MusicPlaylist> playlists = new List<MusicPlaylist>();

            lock (this.playlistsRepository)
            {
                if (googleMusicPlaylists.Playlists != null)
                {
                    foreach (var googleMusicPlaylist in googleMusicPlaylists.Playlists)
                    {
                        var dictionary = (googleMusicPlaylist.Playlist ?? Enumerable.Empty<GoogleMusicSong>()).ToDictionary(x => x.PlaylistEntryId, x => this.songsRepository.AddOrUpdate(x));

                        MusicPlaylist playlist;
                        if (this.playlistsRepository.TryGetValue(Guid.Parse(googleMusicPlaylist.PlaylistId), out playlist))
                        {
                            playlist.Songs.Clear();
                            playlist.Title = googleMusicPlaylist.Title;
                            playlist.EntriesIds.AddRange(dictionary.Keys.ToList());
                            playlist.Songs.AddRange(dictionary.Values.ToList());
                            playlist.CalculateFields();
                        }
                        else
                        {
                            playlist = new MusicPlaylist(
                                Guid.Parse(googleMusicPlaylist.PlaylistId),
                                googleMusicPlaylist.Title,
                                dictionary.Values.ToList(),
                                dictionary.Keys.ToList());
                        }

                        playlists.Add(playlist);
                    }
                }

                var oldPlaylists = this.playlistsRepository.Where(x => googleMusicPlaylists.Playlists.All(np => Guid.Parse(np.PlaylistId) == x.Key)).ToList();
                foreach (var oldPlaylist in oldPlaylists)
                {
                    this.playlistsRepository.Remove(oldPlaylist.Key);
                }
            }

            return playlists;
        }

        private async Task<List<Song>> GetAllSongsTask(IProgress<int> progress = null)
        {
            List<GoogleMusicSong> googleSongs = await this.songWebService.GetAllSongsAsync(progress);

            if (googleSongs != null)
            {
                this.timer = new DispatcherTimer
                                 {
                                     Interval = TimeSpan.FromMinutes(5)
                                 };

                this.timer.Tick += this.SongsUpdate;
                this.timer.Start();

                this.songsRepository.AddRange(googleSongs);
            }

            return this.songsRepository.GetAll().ToList();
        }

        private async void SongsUpdate(object sender, object o)
        {
            var updatedSongs = await this.songWebService.StreamingLoadAllTracksAsync(null);
            if (updatedSongs.Count > 0)
            {
                foreach (var metadata in updatedSongs.Where(m => m.Deleted))
                {
                    this.songsRepository.Remove(metadata.Id);
                }
                
                this.songsRepository.AddRange(updatedSongs.Where(m => !m.Deleted));
            }
        }
    }
}