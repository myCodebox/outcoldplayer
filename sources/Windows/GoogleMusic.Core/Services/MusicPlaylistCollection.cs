// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;

    public class MusicPlaylistCollection : PlaylistCollectionBase<MusicPlaylist>
    {
        private readonly ISongsService songsService;

        public MusicPlaylistCollection(
            ISongsService songsService,
            ISongsRepository songsRepository)
            : base(songsRepository)
        {
            this.songsService = songsService;
        }

        protected override List<MusicPlaylist> Generate()
        {
            // TODO: make async
            var task = this.songsService.GetAllPlaylistsAsync();
            Task.WaitAll(task);
            return task.Result;
        }
    }
}