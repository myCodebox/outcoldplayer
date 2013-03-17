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

    public class AlbumCollection : PlaylistCollectionBase<AlbumBindingModel>
    {
        public AlbumCollection(ISongsRepository songsRepository)
            : base(songsRepository)
        {
        }

        protected override async Task<List<AlbumBindingModel>> LoadCollectionAsync()
        {
            var songs = await this.SongsRepository.GetAllAsync();
            return SongsGrouping.GroupByAlbums(songs).ToList();
        }
    }
}