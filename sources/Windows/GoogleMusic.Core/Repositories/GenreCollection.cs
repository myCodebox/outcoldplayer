// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class GenreCollection : PlaylistCollectionBase<Genre>
    {
        public GenreCollection(ISongsRepository songsRepository)
            : base(songsRepository)
        {
        }

        protected override async Task<List<Genre>> LoadCollectionAsync()
        {
            var songs = await this.SongsRepository.GetAllAsync();
            return songs.GroupBy(x => x.Metadata.Genre)
                     .OrderBy(x => x.Key)
                     .Select(x => new Genre(x.Key, x.ToList()))
                     .ToList();
        }
    }
}