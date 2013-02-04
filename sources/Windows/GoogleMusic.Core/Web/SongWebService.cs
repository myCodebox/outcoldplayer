// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class SongWebService : ISongWebService
    {
        private const string SongUrlFormat = "music/play?u=0&songid={0}";
        private const string RecordPlayingUrl = "music/services/recordplaying";
        private const string ModifyEntriesUrl = "music/services/modifyentries";

        private readonly IGoogleMusicWebService googleMusicWebService;

        public SongWebService(
            IGoogleMusicWebService googleMusicWebService)
        {
            this.googleMusicWebService = googleMusicWebService;
        }

        public async Task<GoogleMusicSongUrl> GetSongUrlAsync(string id)
        {
            var url = string.Format(SongUrlFormat, id);
            return await this.googleMusicWebService.GetAsync<GoogleMusicSongUrl>(url, signUrl: false);
        }

        public async Task<bool> RecordPlayingAsync(GoogleMusicSong song, string playlistId, bool updateRecentAlbum, bool updateRecentPlaylist, int playCount)
        {
            var jsonProperties = new Dictionary<string, string>
                                     {
                                         { "songId", JsonConvert.ToString(song.Id) },
                                         { "playCount", JsonConvert.ToString(playCount) },
                                         { "updateRecentAlbum", JsonConvert.ToString(updateRecentAlbum) },
                                         { "updateRecentPlaylist", JsonConvert.ToString(updateRecentPlaylist) },
                                         { "playlistId", JsonConvert.ToString(playlistId) }
                                     };

            var response = await this.googleMusicWebService.PostAsync<CommonResponse>(RecordPlayingUrl, jsonProperties: jsonProperties);

            return response.Success.HasValue && response.Success.Value;
        }

        public async Task<RatingResp> UpdateRatingAsync(GoogleMusicSong song, int rating)
        {
            var jsonProperties = new Dictionary<string, string>
                                     {
                                         { "id", JsonConvert.ToString(song.Id) },
                                         { "rating", JsonConvert.ToString(rating) }
                                     };

            return await this.googleMusicWebService.PostAsync<RatingResp>(ModifyEntriesUrl, jsonProperties: jsonProperties);
        }
    }
}