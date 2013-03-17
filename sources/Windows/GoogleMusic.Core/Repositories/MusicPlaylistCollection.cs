// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;

    public class MusicPlaylistCollection : PlaylistCollectionBase<UserPlaylistBindingModel>
    {
        private readonly IUserPlaylistRepository userPlaylistRepository;

        public MusicPlaylistCollection(
            IUserPlaylistRepository userPlaylistRepository,
            ISongsRepository songsRepository)
            : base(songsRepository, useCache: false)
        {
            this.userPlaylistRepository = userPlaylistRepository;
        }

        protected async override Task<List<UserPlaylistBindingModel>> LoadCollectionAsync()
        {
            return (await this.userPlaylistRepository.GetAllAsync()).ToList();
        }
    }
}