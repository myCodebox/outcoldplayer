// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class SystemPlaylistCollection : PlaylistCollectionBase<SystemPlaylist>
    {
        private const int HighlyRatedValue = 4;
        private const int EnoughLastAddedSongsCount = 500;

        private readonly IPlaylistCollection<Album> albumCollection;

        public SystemPlaylistCollection(ISongsRepository songsRepository, IPlaylistCollection<Album> albumCollection)
            : base(songsRepository)
        {
            this.albumCollection = albumCollection;
        }

        protected override async Task<List<SystemPlaylist>> LoadCollectionAsync()
        {
            var allSongs = (await this.SongsRepository.GetAllAsync()).ToList();

            var allSongsPlaylist = new SystemPlaylist("All songs", SystemPlaylist.SystemPlaylistType.AllSongs, this.OrderSongs(allSongs));
            var highlyRatedPlaylist = new SystemPlaylist("Highly rated", SystemPlaylist.SystemPlaylistType.HighlyRated, this.OrderSongs(allSongs.Where(x => x.Rating >= HighlyRatedValue)));

            List<Song> lastAddedSongs = new List<Song>();
            var albums = (await this.albumCollection.GetAllAsync()).OrderByDescending(a => a.Songs.Max(s => s.Metadata.CreationDate));
            foreach (var album in albums)
            {
                lastAddedSongs.AddRange(album.Songs);
                if (lastAddedSongs.Count >= EnoughLastAddedSongsCount)
                {
                    break;
                }
            }

            var lastAdded = new SystemPlaylist("Last added", SystemPlaylist.SystemPlaylistType.LastAdded, lastAddedSongs);

            return new List<SystemPlaylist>() { allSongsPlaylist, highlyRatedPlaylist, lastAdded };
        }

        private IEnumerable<Song> OrderSongs(IEnumerable<Song> songs)
        {
            return songs.OrderBy(s => s.Metadata.Artist, StringComparer.CurrentCultureIgnoreCase)
                 .ThenBy(s => s.Metadata.Album, StringComparer.CurrentCultureIgnoreCase)
                 .ThenBy(s => Math.Max(s.Metadata.Disc, (byte)1))
                 .ThenBy(s => s.Metadata.Track);
        }
    }
}