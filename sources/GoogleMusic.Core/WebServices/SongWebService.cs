// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class SongWebService : ISongWebService
    {
        private const string SongUrlFormat = "https://play.google.com/music/play?u=0&songid={0}";
        private const string RecordPlayingUrl = "https://play.google.com/music/services/recordplaying";
        private const string ModifyEntriesUrl = "https://play.google.com/music/services/modifyentries";

        private readonly ILogger logger;
        private readonly IGoogleWebService googleWebService;
        private readonly IUserDataStorage userDataStorage;

        public SongWebService(
            ILogManager logManager,
            IGoogleWebService googleWebService,
            IUserDataStorage userDataStorage)
        {
            this.logger = logManager.CreateLogger("SongWebService");
            this.googleWebService = googleWebService;
            this.userDataStorage = userDataStorage;
        }

        public async Task<GoogleMusicSongUrl> GetSongUrlAsync(string id)
        {
            var url = string.Format(SongUrlFormat, id);
            var response = await this.googleWebService.GetAsync(url);
            
            if (response.HttpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                return response.GetAsJsonObject<GoogleMusicSongUrl>();
            }
            else
            {
                this.logger.Error("Cannot get url for song with id '{0}'.", id);
            }

            return null;
        }

        public async Task<bool> RecordPlayingAsync(GoogleMusicSong song, int playCounts)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          songId = song.Id, 
                                                                                          playCount = playCounts, 
                                                                                          updateRecentAlbum = false,
                                                                                          updateRecentPlaylist = false,
                                                                                          playlistId = song.Title,
                                                                                          sessionId = this.userDataStorage.GetUserSession().SessionId
                                                                                      }) }
                                        };

            var response = await this.googleWebService.PostAsync(RecordPlayingUrl, arguments: requestParameters);
            return response.HttpWebResponse.StatusCode == HttpStatusCode.OK && response.GetAsJsonObject<SuccessResult>().Success;
        }

        public async Task<RatingResp> UpdateRatingAsync(GoogleMusicSong song, int rating)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { 
                                                "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          entries = new []
                                                                                                        {
                                                                                                            new { id = song.Id, rating = rating }
                                                                                                        },
                                                                                          sessionId = this.userDataStorage.GetUserSession().SessionId
                                                                                      }) 
                                            }
                                        };

            var response = await this.googleWebService.PostAsync(ModifyEntriesUrl, arguments: requestParameters);
            if (response.HttpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                return response.GetAsJsonObject<RatingResp>();
            }
            else
            {
                return null;
            }
        }
    }
}