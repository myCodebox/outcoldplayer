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

    public class MusicPlaylistCollection : PlaylistCollectionBase<MusicPlaylist>
    {
        private readonly IMusicPlaylistRepository musicPlaylistRepository;

        public MusicPlaylistCollection(
            IMusicPlaylistRepository musicPlaylistRepository,
            ISongsRepository songsRepository)
            : base(songsRepository, useCache: false)
        {
            this.musicPlaylistRepository = musicPlaylistRepository;
        }

        protected async override Task<List<MusicPlaylist>> LoadCollectionAsync()
        {
            return (await this.musicPlaylistRepository.GetAllAsync()).ToList();
        }
    }
}