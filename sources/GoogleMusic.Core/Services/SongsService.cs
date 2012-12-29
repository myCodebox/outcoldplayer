// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.WebServices;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class SongsService : ISongsService
    {
        private readonly object lockerAllSongs = new object();
        private readonly object lockerAllPlaylists = new object();

        private readonly IPlaylistsWebService webService;

        private readonly IUserDataStorage userDataStorage;

        private Task<List<GoogleMusicSong>> taskAllSongsLoader = null;
        private Task<GoogleMusicPlaylists> taskAllPlaylistsLoader = null;

        private List<Playlist> playlistsCache = null;
        private List<Album> albumsCache = null;
        private List<Genre> genresCache = null;
        private List<Artist> artistsCache = null;

        public SongsService(
            IPlaylistsWebService webService,
            IUserDataStorage userDataStorage)
        {
            this.webService = webService;
            this.userDataStorage = userDataStorage;

            this.userDataStorage.SessionCleared += (sender, args) =>
                {
                    lock (this.lockerAllPlaylists)
                    {
                        this.taskAllPlaylistsLoader = null;
                    }

                    lock (this.lockerAllPlaylists)
                    {
                        this.taskAllSongsLoader = null;
                    }

                    this.albumsCache = null;
                    this.artistsCache = null;
                    this.genresCache = null;
                    this.playlistsCache = null;
                };
        }

        public async Task<List<Album>> GetAllAlbumsAsync(Order order = Order.Name)
        {
            if (this.albumsCache == null)
            {
                var songs = await this.GetAllGoogleSongsAsync();

                this.albumsCache = songs.GroupBy(x => new { x.AlbumNorm, ArtistNorm = string.IsNullOrWhiteSpace(x.AlbumArtistNorm) ? x.ArtistNorm : x.AlbumArtistNorm }).Select(x => new Album(x.ToList())).ToList();
            }

            IEnumerable<Album> enumerable = this.albumsCache;

            if (order == Order.LastPlayed)
            {
                enumerable = enumerable.OrderBy(x => x.Songs.Count > 0 ? x.Songs.Max(s => s.LastPlayed) : double.MaxValue);
            }

            return enumerable.ToList();
        }

        public async Task<List<Playlist>> GetAllPlaylistsAsync(Order order = Order.Name)
        {
            if (this.playlistsCache == null)
            {
                var googleMusicPlaylists = await this.GetAllGooglePlaylistsAsync();

                var playlists = (googleMusicPlaylists.Playlists ?? Enumerable.Empty<GoogleMusicPlaylist>())
                    .Union(googleMusicPlaylists.MagicPlaylists ?? Enumerable.Empty<GoogleMusicPlaylist>());

                this.playlistsCache = playlists.Select(x => new Playlist(x.Title, (x.Playlist ?? Enumerable.Empty<GoogleMusicSong>()).ToList())).ToList();
            }

            IEnumerable<Playlist> enumerable = this.playlistsCache;

            if (order == Order.LastPlayed)
            {
                enumerable = enumerable.OrderBy(x => x.Songs.Count > 0 ? x.Songs.Max(s => s.LastPlayed) : double.MaxValue);
            }

            return enumerable.ToList();
        }

        public async Task<List<Genre>> GetAllGenresAsync(Order order = Order.Name)
        {
            if (this.genresCache == null)
            {
                var songs = await this.GetAllGoogleSongsAsync();

                this.genresCache = songs.GroupBy(x => x.Genre).OrderBy(x => x.Key).Select(x => new Genre(x.Key, x.ToList())).ToList();
            }

            IEnumerable<Genre> enumerable = this.genresCache;

            if (order == Order.LastPlayed)
            {
                enumerable = enumerable.OrderBy(x => x.Songs.Count > 0 ? x.Songs.Max(s => s.LastPlayed) : double.MaxValue);
            }

            return enumerable.ToList();
        }

        public async Task<List<Artist>> GetAllArtistsAsync(Order order = Order.Name)
        {
            if (this.artistsCache == null)
            {
                var songs = await this.GetAllGoogleSongsAsync();

                this.artistsCache = songs.GroupBy(x => string.IsNullOrWhiteSpace(x.AlbumArtistNorm) ? x.ArtistNorm : x.AlbumArtistNorm).OrderBy(x => x.Key).Select(x => new Artist(x.ToList())).ToList();
            }

            IEnumerable<Artist> enumerable = this.artistsCache;

            if (order == Order.LastPlayed)
            {
                enumerable = enumerable.OrderBy(x => x.Songs.Count > 0 ? x.Songs.Max(s => s.LastPlayed) : double.MaxValue);
            }

            return enumerable.ToList();
        }

        private async Task<List<GoogleMusicSong>> GetAllGoogleSongsAsync()
        {
            await this.LoadAllAsync();
            return await this.GetAllSongsTask();
        }

        private async Task<GoogleMusicPlaylists> GetAllGooglePlaylistsAsync()
        {
            await this.LoadAllAsync();
            return await this.GetAllPlaylistsTask();
        }

        private async Task LoadAllAsync()
        {
            await this.GetAllPlaylistsTask();
            await this.GetAllSongsTask();
        }

        private Task<GoogleMusicPlaylists> GetAllPlaylistsTask()
        {
            lock (this.lockerAllPlaylists)
            {
                if (this.taskAllPlaylistsLoader == null)
                {
                    this.taskAllPlaylistsLoader = this.webService.GetAllPlaylistsAsync();
                }
            }

            return this.taskAllPlaylistsLoader;
        }

        private Task<List<GoogleMusicSong>> GetAllSongsTask()
        {
            lock (this.lockerAllSongs)
            {
                if (this.taskAllSongsLoader == null)
                {
                    this.taskAllSongsLoader = this.webService.GetAllSongsAsync();
                }
            }

            return this.taskAllSongsLoader;
        }
    }
}