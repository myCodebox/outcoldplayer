// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public interface ISongWebService
    {
        Task<GoogleMusicSongUrl> GetSongUrlAsync(string id);

        Task<bool> RecordPlayingAsync(GoogleMusicSong song, int playCounts);
    }
}