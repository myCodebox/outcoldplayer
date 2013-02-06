// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public class GenreCollection : PlaylistCollectionBase<Genre>
    {
        public GenreCollection(ISongsRepository songsRepository)
            : base(songsRepository)
        {
        }

        protected override Task<List<Genre>> LoadCollectionAsync()
        {
            return Task.FromResult(this.SongsRepository.GetAll()
                    .GroupBy(x => x.GoogleMusicMetadata.Genre)
                    .OrderBy(x => x.Key)
                    .Select(x => new Genre(x.Key, x.ToList())).ToList());
        }
    }
}