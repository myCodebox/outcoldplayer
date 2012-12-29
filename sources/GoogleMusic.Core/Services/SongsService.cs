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

        private Task<List<GoogleMusicSong>> taskAllSongsLoader = null;
        private Task<GoogleMusicPlaylists> taskAllPlaylistsLoader = null;

        private List<Playlist> playlistsCache = null;
        private List<Album> albumsCache = null;

        public SongsService(IPlaylistsWebService webService)
        {
            this.webService = webService;
        }

        public async Task<List<Album>> GetAllAlbumsAsync(Order order = Order.Name)
        {
            if (this.albumsCache != null)
            {
                var songs = await this.GetAllGoogleSongsAsync();

                this.albumsCache = songs.GroupBy(x => new { x.AlbumNorm, ArtistNorm = x.AlbumArtistNorm ?? x.ArtistNorm }).Select(x => new Album(x.ToList())).ToList();
            }

            IEnumerable<Album> enumerable = this.albumsCache;

            if (order == Order.LastPlayed)
            {
                enumerable = enumerable.OrderBy(x => x.Songs.Max(s => s.LastPlayed));
            }

            return enumerable.ToList();
        }

        public async Task<List<Playlist>> GetAllPlaylistsAsync(Order order = Order.Name)
        {
            if (this.playlistsCache != null)
            {
                var googleMusicPlaylists = await this.GetAllGooglePlaylistsAsync();

                var playlists = (googleMusicPlaylists.Playlists ?? Enumerable.Empty<GoogleMusicPlaylist>())
                    .Union(googleMusicPlaylists.MagicPlaylists ?? Enumerable.Empty<GoogleMusicPlaylist>());

                this.playlistsCache = playlists.Select(x => new Playlist(x.Title, (x.Playlist ?? Enumerable.Empty<GoogleMusicSong>()).ToList())).ToList();
            }

            IEnumerable<Playlist> enumerable = this.playlistsCache;

            if (order == Order.LastPlayed)
            {
                enumerable = enumerable.OrderBy(x => x.Songs.Max(s => s.LastPlayed));
            }

            return enumerable.ToList();
        }

        private async Task<List<GoogleMusicSong>> GetAllGoogleSongsAsync()
        {
            lock (this.lockerAllSongs)
            {
                if (this.taskAllSongsLoader == null)
                {
                    this.taskAllSongsLoader = this.webService.GetAllSongsAsync();
                }
            }

            return await this.taskAllSongsLoader;
        }

        private async Task<GoogleMusicPlaylists> GetAllGooglePlaylistsAsync()
        {
            lock (this.lockerAllPlaylists)
            {
                if (this.taskAllPlaylistsLoader == null)
                {
                    this.taskAllPlaylistsLoader = this.webService.GetAllPlaylistsAsync();
                }
            }

            return await this.taskAllPlaylistsLoader;
        }
    }
}