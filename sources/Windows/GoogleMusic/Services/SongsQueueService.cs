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

        public async Task PlayAsync(IPlaylist playlist)
        {
            if (playlist == null)
            {
                throw new ArgumentNullException("playlist");
            }

            IList<Song> songs = null;

            if (playlist is Album)
            {
                songs = await this.songsRepository.GetAlbumSongsAsync((Album)playlist);
            }
            else if (playlist is Artist)
            {
                songs = await this.songsRepository.GetArtistSongsAsync((Artist)playlist);
            }
            else if (playlist is UserPlaylist)
            {
                songs = await this.songsRepository.GetUserPlaylistSongsAsync((UserPlaylist)playlist);
            }
            else if (playlist is Genre)
            {
                songs = await this.songsRepository.GetGenreSongsAsync((Genre)playlist);
            }
            else
            {
                // TODO: Implement system playlists
                throw new ArgumentException("Unknown songs container type.", "playlist");
            }

            await this.playQueueService.PlayAsync(playlist, songs);
        }
    }
}
