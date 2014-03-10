// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;

    public abstract class AllAccessServiceBase
    {
        private readonly ISongsRepository songsRepository;

        protected AllAccessServiceBase(ISongsRepository songsRepository)
        {
            this.songsRepository = songsRepository;
        }

        protected async Task<Song> GetSong(GoogleMusicSong googleMusicSong)
        {
            Song song = googleMusicSong.ToSong();

            Song storedSong = await this.songsRepository.FindSongAsync(song.SongId);

            if (storedSong == null)
            {
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
