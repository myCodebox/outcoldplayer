// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface ISongWebService
    {
        Task<GoogleMusicSongUrl> GetSongUrlAsync(Guid id);

        Task<bool> RecordPlayingAsync(GoogleMusicSong song, string playlistId, bool updateRecentAlbum, bool updateRecentPlaylist, int playCount);

        Task<RatingResp> UpdateRatingAsync(GoogleMusicSong song, int rating);
    }
}