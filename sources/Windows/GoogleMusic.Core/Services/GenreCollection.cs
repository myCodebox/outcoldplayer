// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;

    public class GenreCollection : PlaylistCollectionBase<Genre>, IGenreCollection
    {
        public GenreCollection(ISongsRepository songsRepository)
            : base(songsRepository)
        {
        }

        protected override List<Genre> Generate()
        {
            return this.SongsRepository.GetAll()
                    .GroupBy(x => x.GoogleMusicMetadata.Genre)
                    .OrderBy(x => x.Key)
                    .Select(x => new Genre(x.Key, x.ToList())).ToList();
        }
    }
}