// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface ISongWebService
    {
        Task<GoogleMusicSongUrl> GetSongUrlAsync(string id);

        Task<bool> RecordPlayingAsync(GoogleMusicSong song, int playCounts);

        Task<RatingResp> UpdateRatingAsync(GoogleMusicSong song, int rating);
    }
}