// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public class AlbumCollection : PlaylistCollectionBase<Album>
    {
        public AlbumCollection(ISongsRepository songsRepository)
            : base(songsRepository)
        {
        }

        protected override Task<List<Album>> LoadCollectionAsync()
        {
            return Task.FromResult(SongsGrouping.GroupByAlbums(this.SongsRepository.GetAll()).ToList());
        }
    }
}