// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;

    public abstract class AllAccessServiceBase
    {
        private readonly ISongsRepository songsRepository;
        private readonly IRatingCacheService ratingCacheService;

        protected AllAccessServiceBase(ISongsRepository songsRepository, IRatingCacheService ratingCacheService)
        {
            this.songsRepository = songsRepository;
            this.ratingCacheService = ratingCacheService;
        }

        protected async Task<Song> GetSong(GoogleMusicSong googleMusicSong)
        {
            Song song = googleMusicSong.ToSong();

            Song storedSong = await this.songsRepository.FindSongAsync(song.SongId);

            if (storedSong == null)
            {
                Tuple<DateTime, byte> cachedRating = this.ratingCacheService.GetCachedRating(song);
                if (cachedRating != null && cachedRating.Item1 > song.LastModified)
                {
                    song.Rating = cachedRating.Item2;
                }

                song.IsLibrary = false;
                song.UnknownSong = true;
                return song;
            }
            else
            {
                return storedSong;
            }
        }
    }
}
