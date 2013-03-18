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
        private readonly IUserPlaylistsRepository userPlaylistsRepository;

        public MusicPlaylistCollection(
            IUserPlaylistsRepository userPlaylistsRepository,
            ISongsRepository songsRepository)
            : base(songsRepository, useCache: false)
        {
            this.userPlaylistsRepository = userPlaylistsRepository;
        }

        protected async override Task<List<UserPlaylistBindingModel>> LoadCollectionAsync()
        {
            return (await this.userPlaylistsRepository.GetAllAsync()).ToList();
        }
    }
}