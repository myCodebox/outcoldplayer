// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;

    public class GenreCollection : PlaylistCollectionBase<GenreBindingModel>
    {
        public GenreCollection(ISongsRepository songsRepository)
            : base(songsRepository)
        {
        }

        protected override async Task<List<GenreBindingModel>> LoadCollectionAsync()
        {
            var songs = await this.SongsRepository.GetAllAsync();
            return songs.GroupBy(x => x.Metadata.Genre.Title)
                     .OrderBy(x => x.Key)
                     .Select(x => new GenreBindingModel(x.Key, x.ToList()))
                     .ToList();
        }
    }
}