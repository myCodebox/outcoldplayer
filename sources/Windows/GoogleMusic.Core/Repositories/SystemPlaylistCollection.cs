// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public class SystemPlaylistCollection : PlaylistCollectionBase<SystemPlaylist>
    {
        private const int HighlyRatedValue = 4;

        public SystemPlaylistCollection(ISongsRepository songsRepository)
            : base(songsRepository)
        {
        }

        protected override Task<List<SystemPlaylist>> LoadCollectionAsync()
        {
            var allSongs = this.SongsRepository.GetAll().ToList();

            var allSongsPlaylist = new SystemPlaylist("All songs", SystemPlaylist.SystemPlaylistType.AllSongs, allSongs);
            var highlyRatedPlaylist = new SystemPlaylist("Highly rated", SystemPlaylist.SystemPlaylistType.HighlyRated, allSongs.Where(x => x.Rating >= HighlyRatedValue));

            return Task.FromResult(new List<SystemPlaylist>() { allSongsPlaylist, highlyRatedPlaylist });
        }
    }
}