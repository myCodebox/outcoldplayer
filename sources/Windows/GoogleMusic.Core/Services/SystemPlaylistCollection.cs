// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;

    public class SystemPlaylistCollection : PlaylistCollectionBase<SystemPlaylist>
    {
        private const int HighlyRatedValue = 4;

        public SystemPlaylistCollection(ISongsRepository songsRepository)
            : base(songsRepository)
        {
        }

        protected override List<SystemPlaylist> Generate()
        {
            var allSongs = this.SongsRepository.GetAll().ToList();

            var allSongsPlaylist = new SystemPlaylist("All songs", SystemPlaylist.SystemPlaylistType.AllSongs, allSongs);
            var highlyRatedPlaylist = new SystemPlaylist("Highly rated", SystemPlaylist.SystemPlaylistType.HighlyRated, allSongs.Where(x => x.Rating >= HighlyRatedValue));

            return new List<SystemPlaylist>() { allSongsPlaylist, highlyRatedPlaylist };
        }
    }
}