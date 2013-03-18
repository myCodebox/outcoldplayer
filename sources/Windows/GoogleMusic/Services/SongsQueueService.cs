// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public class SongsQueueService : ISongsQueueService
    {
        private readonly IPlayQueueService playQueueService;
        private readonly ISongsRepository songsRepository;

        public SongsQueueService(
            IPlayQueueService playQueueService, 
            ISongsRepository songsRepository)
        {
            this.playQueueService = playQueueService;
            this.songsRepository = songsRepository;
        }

        public async Task PlayAsync(ISongsContainer songsContainer)
        {
            if (songsContainer == null)
            {
                throw new ArgumentNullException("songsContainer");
            }

            IList<Song> songs = null;

            if (songsContainer is Album)
            {
                songs = await this.songsRepository.GetAlbumSongsAsync((Album)songsContainer);
            }
            else if (songsContainer is Artist)
            {
                songs = await this.songsRepository.GetArtistSongsAsync((Artist)songsContainer);
            }
            else if (songsContainer is UserPlaylist)
            {
                songs = await this.songsRepository.GetUserPlaylistSongsAsync((UserPlaylist)songsContainer);
            }
            else if (songsContainer is Genre)
            {
                songs = await this.songsRepository.GetGenreSongsAsync((Genre)songsContainer);
            }
            else
            {
                // TODO: Implement system playlists
                throw new ArgumentException("Unknown songs container type.", "songsContainer");
            }

            await this.playQueueService.PlayAsync(songsContainer, songs);
        }
    }
}
