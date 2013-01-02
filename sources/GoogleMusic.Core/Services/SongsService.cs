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

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.WebServices;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class SongsService : ISongsService
    {
        private readonly object lockerTasks = new object();

        private readonly Dictionary<string, Song> songsRepository = new Dictionary<string, Song>();

        private readonly IPlaylistsWebService webService;
        private readonly ISongWebService songWebService;
        private readonly IUserDataStorage userDataStorage;

        private Task<List<Song>> taskAllSongsLoader = null;
        private Task<List<MusicPlaylist>> taskAllPlaylistsLoader = null;

        private List<MusicPlaylist> playlistsCache = null;
        private List<Album> albumsCache = null;
        private List<Genre> genresCache = null;
        private List<Artist> artistsCache = null;

        public SongsService(
            IPlaylistsWebService webService,
            IUserDataStorage userDataStorage,
            ISongWebService songWebService)
        {
            this.webService = webService;
            this.userDataStorage = userDataStorage;
            this.songWebService = songWebService;

            this.userDataStorage.SessionCleared += (sender, args) =>
                {
                    lock (this.lockerTasks)
                    {
                        this.taskAllPlaylistsLoader = null;
                        this.taskAllSongsLoader = null;
                    }

                    this.albumsCache = null;
                    this.artistsCache = null;
                    this.genresCache = null;
                    this.playlistsCache = null;

                    foreach (var song in this.songsRepository)
                    {
                        song.Value.PropertyChanged -= this.SongOnPropertyChanged;
                    }

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

        public async Task<List<MusicPlaylist>> GetAllPlaylistsAsync(Order order = Order.Name)
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

        public async Task<MusicPlaylist> CreatePlaylistAsync()
        {
            var name = string.Format(CultureInfo.CurrentCulture, "Playlist - {0}", DateTime.Now);
            var resp = await this.webService.CreatePlaylistAsync(name);
            if (resp != null && !string.IsNullOrEmpty(resp.Id))
            {
                var musicPlaylist = new MusicPlaylist(resp.Id, resp.Title, new List<Song>(), new List<string>());
                this.playlistsCache.Add(musicPlaylist);
                return musicPlaylist;
            }

            return null;
        }

        public async Task<bool> DeletePlaylistAsync(MusicPlaylist playlist)
        {
            bool result = await this.webService.DeletePlaylistAsync(playlist.Id);

            if (result)
            {
                this.playlistsCache.Remove(playlist);
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
                playlist.EntriesIds.Add(result.SongIds[0].PlaylisEntryId);
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
                playlists = playlists.OrderBy(x => x.Songs.Count > 0 ? x.Songs.Max(s => s.GoogleMusicMetadata.LastPlayed) : double.MaxValue);
            }

            return playlists;
        }

        private async Task<List<MusicPlaylist>> GetAllGooglePlaylistsAsync()
        {
            lock (this.lockerTasks)
            {
                if (this.taskAllPlaylistsLoader == null)
                {
                    this.taskAllPlaylistsLoader = this.GetAllPlaylistsTask();
                }
            }
            
            return await this.taskAllPlaylistsLoader;
        }

        private async Task<List<MusicPlaylist>> GetAllPlaylistsTask()
        {
            var googleMusicPlaylists = await this.webService.GetAllPlaylistsAsync();

            var query =
                (googleMusicPlaylists.Playlists ?? Enumerable.Empty<GoogleMusicPlaylist>()).Union(
                    googleMusicPlaylists.MagicPlaylists ?? Enumerable.Empty<GoogleMusicPlaylist>());

            List<MusicPlaylist> playlists = new List<MusicPlaylist>();

            foreach (var googleMusicPlaylist in query)
            {
                var dictionary = (googleMusicPlaylist.Playlist ?? Enumerable.Empty<GoogleMusicSong>()).ToDictionary(x => x.PlaylistEntryId, this.CreateSong);
                playlists.Add(new MusicPlaylist(googleMusicPlaylist.PlaylistId, googleMusicPlaylist.Title, dictionary.Values.ToList(), dictionary.Keys.ToList()));
            }

            return playlists;
        }

        private async Task<List<Song>> GetAllSongsTask(IProgress<int> progress = null)
        {
            var googleSongs = await this.webService.GetAllSongsAsync(progress);
            return googleSongs.Select(this.CreateSong).ToList();
        }

        private Song CreateSong(GoogleMusicSong googleSong)
        {
            Song song;
            lock (this.songsRepository)
            {
                if (!this.songsRepository.TryGetValue(googleSong.Id, out song))
                {
                    song = new Song(googleSong);
                    song.PropertyChanged += this.SongOnPropertyChanged;
                    this.songsRepository.Add(googleSong.Id, song);
                }
            }

            return song;
        }

        private void SongOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (string.Equals(propertyChangedEventArgs.PropertyName, "Rating"))
            {
                var song = (Song)sender;
                this.songWebService.UpdateRatingAsync(song.GoogleMusicMetadata, song.Rating).ContinueWith(
                    t =>
                        {
                            if (t.IsCompleted && t.Result != null)
                            {
                                var songRatingResp = t.Result.Songs.FirstOrDefault(x => string.Equals(x.Id, song.GoogleMusicMetadata.Id, StringComparison.OrdinalIgnoreCase));
                                if (songRatingResp != null)
                                {
                                    song.Rating = songRatingResp.Rating;
                                }
                            }
                        },
                    TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
    }
}