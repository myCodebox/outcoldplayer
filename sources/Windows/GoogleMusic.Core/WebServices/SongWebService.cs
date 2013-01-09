// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class SongWebService : ISongWebService
    {
        private const string SongUrlFormat = "play?u=0&songid={0}";
        private const string RecordPlayingUrl = "https://play.google.com/music/services/recordplaying";
        private const string ModifyEntriesUrl = "https://play.google.com/music/services/modifyentries";

        private readonly ILogger logger;
        private readonly IGoogleMusicWebService googleMusicWebService;
        private readonly IUserDataStorage userDataStorage;

        public SongWebService(
            ILogManager logManager,
            IGoogleMusicWebService googleMusicWebService,
            IUserDataStorage userDataStorage)
        {
            this.logger = logManager.CreateLogger("SongWebService");
            this.googleMusicWebService = googleMusicWebService;
            this.userDataStorage = userDataStorage;
        }

        public async Task<GoogleMusicSongUrl> GetSongUrlAsync(string id)
        {
            var url = string.Format(SongUrlFormat, id);
            var response = await this.googleMusicWebService.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("GetSongUrlAsync: Response JSON: '{0}'.", responseString);
                }

                try
                {
                    return JsonConvert.DeserializeObject<GoogleMusicSongUrl>(responseString);
                }
                catch (Exception e)
                {
                    this.logger.Error("Canot deserialize json data");
                    this.logger.LogErrorException(e);
                }
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

            var response = await this.googleMusicWebService.PostAsync(RecordPlayingUrl, arguments: requestParameters);
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

            var response = await this.googleMusicWebService.PostAsync(ModifyEntriesUrl, arguments: requestParameters);
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