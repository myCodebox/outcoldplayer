// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface ISongWebService
    {
        Task<StatusResp> GetStatusAsync();

        Task<List<GoogleMusicSong>> GetAllSongsAsync(IProgress<int> progress = null);

        Task<List<GoogleMusicSong>> StreamingLoadAllTracksAsync(IProgress<int> progress);

        Task<GoogleMusicSongUrl> GetSongUrlAsync(Guid id);

        Task<bool> RecordPlayingAsync(GoogleMusicSong song, string playlistId, bool updateRecentAlbum, bool updateRecentPlaylist, int playCount);

        Task<RatingResp> UpdateRatingAsync(GoogleMusicSong song, int rating);
    }
}