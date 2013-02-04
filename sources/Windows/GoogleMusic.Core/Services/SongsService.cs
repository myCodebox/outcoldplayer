// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    using Windows.UI.Xaml;

    public class SongsService : ISongsService
    {
        private const int HighlyRatedValue = 4;

        private readonly object lockerTasks = new object();

        private readonly Dictionary<Guid, Song> songsRepository = new Dictionary<Guid, Song>();
        private readonly Dictionary<Guid, MusicPlaylist> playlistsRepository = new Dictionary<Guid, MusicPlaylist>();

        private readonly ILogger logger;

        private readonly IPlaylistsWebService webService;

        private readonly IGoogleMusicSessionService sessionService;

        private readonly ISongWebService songWebService;

        private Task<List<Song>> taskAllSongsLoader = null;
        private Task<List<MusicPlaylist>> taskAllPlaylistsLoader = null;

        private DispatcherTimer timer;

        private DateTime? lastPlaylistsLoad;

        public SongsService(
            IPlaylistsWebService webService,
            IGoogleMusicSessionService sessionService,
            ISongWebService songWebService,
            ILogManager logManager)
        {
            this.webService = webService;
            this.sessionService = sessionService;
            this.songWebService = songWebService;
            this.logger = logManager.CreateLogger("SongsService");

            this.sessionService.SessionCleared += (sender, args) =>
                {
                    this.timer.Stop();
                    this.timer = null;

                    lock (this.lockerTasks)
                    {
                        this.taskAllPlaylistsLoader = null;
                        this.taskAllSongsLoader = null;
                    }

                    foreach (var song in this.songsRepository)
                    {
                        var songValue = song.Value;
                        song.Value.Unsubscribe(() => songValue.Rating, this.SongOnPropertyChanged);
                    }

                    this.songsRepository.Clear();
                };
        }

        public Task<List<Album>> GetAllAlbumsAsync(Order order = Order.Name)
        {
            return Task.Factory.StartNew(() =>
                {
                    var albums = this.songsRepository.Values
                        .GroupBy(x => new
                                          {
                                              x.GoogleMusicMetadata.AlbumNorm, 
                                              ArtistNorm = string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.AlbumArtistNorm) 
                                                                    ? x.GoogleMusicMetadata.ArtistNorm 
                                                                    : x.GoogleMusicMetadata.AlbumArtistNorm
                                          })
                        .Select(x => new Album(x.ToList()));

                    return OrderCollection(albums, order).ToList();
                });
        }

        public async Task<List<MusicPlaylist>> GetAllPlaylistsAsync(Order order = Order.Name, bool canReload = false)
        {
            return OrderCollection(await this.GetAllGooglePlaylistsAsync(canReload), order).ToList();
        }

        public Task<List<Genre>> GetAllGenresAsync(Order order = Order.Name)
        {
            return Task.Factory.StartNew(
                () =>
                    {
                        var genresCache = this.songsRepository.Values
                                .GroupBy(x => x.GoogleMusicMetadata.Genre)
                                .OrderBy(x => x.Key)
                                .Select(x => new Genre(x.Key, x.ToList()));

                        return OrderCollection(genresCache, order).ToList();
                    });
        }

        public Task<List<Artist>> GetAllArtistsAsync(Order order = Order.Name, bool includeNotAlbums = false)
        {
            return Task.Factory.StartNew(
                () =>
                    {
                        var artistsCache = this.songsRepository.Values
                            .GroupBy(x => string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.AlbumArtistNorm) ? x.GoogleMusicMetadata.ArtistNorm : x.GoogleMusicMetadata.AlbumArtistNorm)
                            .OrderBy(x => x.Key)
                            .Select(x => new Artist(x.ToList()));

                        if (includeNotAlbums)
                        {
                            var artists = artistsCache.ToList();

                            var groupBy = this.songsRepository.Values.GroupBy(x => x.GoogleMusicMetadata.ArtistNorm);
                            foreach (var group in groupBy)
                            {
                                var artist = artists.FirstOrDefault(
                                    x => string.Equals(group.Key, x.Title, StringComparison.CurrentCultureIgnoreCase));

                                if (artist != null)
                                {
                                    foreach (Song song in group)
                                    {
                                        if (!artist.Songs.Contains(song))
                                        {
                                            artist.Songs.Add(song);
                                        }
                                    }
                                }
                                else
                                {
                                    artists.Add(new Artist(group.ToList(), useArtist: true));
                                }
                            }
                            
                            artistsCache = artists;
                        }

                        return OrderCollection(artistsCache, order).ToList();
                    });
        }

        public async Task<MusicPlaylist> CreatePlaylistAsync()
        {
            var name = string.Format(CultureInfo.CurrentCulture, "Playlist - {0}", DateTime.Now);
            var resp = await this.webService.CreatePlaylistAsync(name);
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
            bool result = await this.webService.DeletePlaylistAsync(playlist.Id);

            if (result)
            {
                this.lastPlaylistsLoad = null;
                await this.GetAllGooglePlaylistsAsync(canReload: true);
            }

            return result;
        }

        public async Task<bool> ChangePlaylistNameAsync(MusicPlaylist playlist, string newName)
        {
            var result = await this.webService.ChangePlaylistNameAsync(playlist.Id, newName);

            if (result)
            {
                playlist.Title = newName;
            }

            return result;
        }

        public async Task<bool> RemoveSongFromPlaylistAsync(MusicPlaylist playlist, int index)
        {
            bool result = await this.webService.RemoveSongFromPlaylistAsync(playlist.Id, playlist.Songs[index].GoogleMusicMetadata.Id, playlist.EntriesIds[index]);
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
            var result = await this.webService.AddSongToPlaylistAsync(playlist.Id, song.GoogleMusicMetadata.Id);
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
            var googleMusicPlaylists = await this.webService.GetAllPlaylistsAsync();

            List<MusicPlaylist> playlists = new List<MusicPlaylist>();

            lock (this.playlistsRepository)
            {
                if (googleMusicPlaylists.Playlists != null)
                {
                    foreach (var googleMusicPlaylist in googleMusicPlaylists.Playlists)
                    {
                        var dictionary = (googleMusicPlaylist.Playlist ?? Enumerable.Empty<GoogleMusicSong>()).ToDictionary(x => x.PlaylistEntryId, x => this.CreateSong(x));

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
            List<GoogleMusicSong> googleSongs = await this.webService.GetAllSongsAsync(progress);

            if (googleSongs != null)
            {
                this.timer = new DispatcherTimer
                                 {
                                     Interval = TimeSpan.FromMinutes(5)
                                 };

                this.timer.Tick += this.SongsUpdate;
                this.timer.Start();
            }

            return googleSongs.Select(x => this.CreateSong(x)).ToList();
        }

        private async void SongsUpdate(object sender, object o)
        {
            var updatedSongs = await this.webService.StreamingLoadAllTracksAsync(null);
            if (updatedSongs.Count > 0)
            {
                foreach (var s in updatedSongs)
                {
                    this.CreateSong(s, updateSong: true);
                }
            }
        }

        private Song CreateSong(GoogleMusicSong googleSong, bool updateSong = false)
        {
            Song song;
            lock (this.songsRepository)
            {
                if (!this.songsRepository.TryGetValue(googleSong.Id, out song))
                {
                    song = new Song(googleSong);
                    song.Subscribe(() => song.Rating, this.SongOnPropertyChanged);
                    this.songsRepository.Add(googleSong.Id, song);
                }
                else if (updateSong)
                {
                    if (googleSong.Deleted)
                    {
                        this.songsRepository.Remove(googleSong.Id);
                    }
                    else
                    {
                        song.Unsubscribe(() => song.Rating, this.SongOnPropertyChanged);
                        song.GoogleMusicMetadata = googleSong;
                        song.Title = googleSong.Title;
                        song.Duration = TimeSpan.FromMilliseconds(googleSong.DurationMillis).TotalSeconds;
                        song.Artist = googleSong.Artist;
                        song.Album = googleSong.Album;
                        song.PlayCount = googleSong.PlayCount;
                        song.Rating = googleSong.Rating;
                        song.Subscribe(() => song.Rating, this.SongOnPropertyChanged);
                    }
                }
            }

            return song;
        }

        private void SongOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (string.Equals(propertyChangedEventArgs.PropertyName, "Rating"))
            {
                var song = (Song)sender;

                this.logger.Debug("Rating is changed for song '{0}'. Updating server.", song.GoogleMusicMetadata.Id);

                if (song.Rating != song.GoogleMusicMetadata.Rating)
                {
                    this.songWebService.UpdateRatingAsync(song.GoogleMusicMetadata, song.Rating).ContinueWith(
                        t =>
                            {
                                if (t.IsCompleted && !t.IsFaulted && t.Result != null)
                                {
                                    if (this.logger.IsDebugEnabled)
                                    {
                                        this.logger.Debug(
                                            "Rating update completed for song: {0}.", song.GoogleMusicMetadata.Id);
                                        foreach (var songUpdate in t.Result.Songs)
                                        {
                                            this.logger.Debug(
                                                "Song updated: {0}, Rate: {1}.", songUpdate.Id, songUpdate.Rating);
                                        }
                                    }
                                }
                                else
                                {
                                    this.logger.Debug(
                                        "Failed to update rating for song: {0}.", song.GoogleMusicMetadata.Id);
                                    if (t.IsFaulted && t.Exception != null)
                                    {
                                        this.logger.LogErrorException(t.Exception);
                                    }
                                }
                            });
                }
            }
        }
    }
}