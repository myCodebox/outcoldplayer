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

        private readonly Dictionary<string, Song> songsRepository = new Dictionary<string, Song>();

        private readonly IPlaylistsWebService webService;

        private readonly IUserDataStorage userDataStorage;

        private Task<List<Song>> taskAllSongsLoader = null;
        private Task<List<Playlist>> taskAllPlaylistsLoader = null;

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

                    this.songsRepository.Clear();
                };
        }

        public async Task<List<Album>> GetAllAlbumsAsync(Order order = Order.Name)
        {
            if (this.albumsCache == null)
            {
                var songs = await this.GetAllGoogleSongsAsync();

                this.albumsCache = songs.GroupBy(x => new { x.GoogleMusicMetadata.AlbumNorm, ArtistNorm = string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.AlbumArtistNorm) ? x.GoogleMusicMetadata.ArtistNorm : x.GoogleMusicMetadata.AlbumArtistNorm }).Select(x => new Album(x.ToList())).ToList();
            }

            return OrderCollection(this.albumsCache, order).ToList();
        }

        public async Task<List<Playlist>> GetAllPlaylistsAsync(Order order = Order.Name)
        {
            if (this.playlistsCache == null)
            {
                this.playlistsCache = await this.GetAllGooglePlaylistsAsync();
            }
            
            return OrderCollection(this.playlistsCache, order).ToList();
        }

        public async Task<List<Genre>> GetAllGenresAsync(Order order = Order.Name)
        {
            if (this.genresCache == null)
            {
                var songs = await this.GetAllGoogleSongsAsync();

                this.genresCache = songs.GroupBy(x => x.GoogleMusicMetadata.Genre).OrderBy(x => x.Key).Select(x => new Genre(x.Key, x.ToList())).ToList();
            }

            return OrderCollection(this.genresCache, order).ToList();
        }

        public async Task<List<Artist>> GetAllArtistsAsync(Order order = Order.Name)
        {
            if (this.artistsCache == null)
            {
                var songs = await this.GetAllGoogleSongsAsync();

                this.artistsCache = songs.GroupBy(x => string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.AlbumArtistNorm) ? x.GoogleMusicMetadata.ArtistNorm : x.GoogleMusicMetadata.AlbumArtistNorm).OrderBy(x => x.Key).Select(x => new Artist(x.ToList())).ToList();
            }

            return OrderCollection(this.artistsCache, order).ToList();
        }

        private IEnumerable<TPlaylist> OrderCollection<TPlaylist>(IEnumerable<TPlaylist> playlists, Order order)
            where TPlaylist : Playlist
        {
            if (order == Order.LastPlayed)
            {
                playlists = playlists.OrderBy(x => x.Songs.Count > 0 ? x.Songs.Max(s => s.GoogleMusicMetadata.LastPlayed) : double.MaxValue);
            }

            return playlists;
        }

        private Task<List<Song>> GetAllGoogleSongsAsync()
        {
            lock (this.lockerAllSongs)
            {
                if (this.taskAllSongsLoader == null)
                {
                    this.taskAllSongsLoader = this.GetAllSongsTask();
                }
            }

            return this.taskAllSongsLoader;
        }

        private Task<List<Playlist>> GetAllGooglePlaylistsAsync()
        {
            lock (this.lockerAllPlaylists)
            {
                if (this.taskAllPlaylistsLoader == null)
                {
                    this.taskAllPlaylistsLoader = this.GetAllPlaylistsTask();
                }
            }

            return this.taskAllPlaylistsLoader;
        }

        private async Task<List<Playlist>> GetAllPlaylistsTask()
        {
            var googleMusicPlaylists = await this.webService.GetAllPlaylistsAsync();

            var playlists =
                (googleMusicPlaylists.Playlists ?? Enumerable.Empty<GoogleMusicPlaylist>()).Union(
                    googleMusicPlaylists.MagicPlaylists ?? Enumerable.Empty<GoogleMusicPlaylist>());

            return playlists.Select(
                    x =>
                    new Playlist(x.Title, (x.Playlist ?? Enumerable.Empty<GoogleMusicSong>()).Select(this.CreateSong).ToList())).ToList();
        }

        private async Task<List<Song>> GetAllSongsTask()
        {
            var googleSongs = await this.webService.GetAllSongsAsync();
            return googleSongs.Select(this.CreateSong).ToList();
        }

        private Song CreateSong(GoogleMusicSong googleSong)
        {
            Song song;
            lock (this.songsRepository)
            {
                if (!this.songsRepository.TryGetValue(googleSong.Id, out song))
                {
                    this.songsRepository.Add(googleSong.Id, song = new Song(googleSong));
                }
            }

            return song;
        }
    }
}