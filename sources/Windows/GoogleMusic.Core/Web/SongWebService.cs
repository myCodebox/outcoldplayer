// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class SongWebService : ISongWebService
    {
        private const string SongUrlFormat = "music/play?u=0&songid={0}";
        private const string RecordPlayingUrl = "music/services/recordplaying";
        private const string ModifyEntriesUrl = "music/services/modifyentries";

        private readonly ILogger logger;
        private readonly IGoogleMusicWebService googleMusicWebService;
        private readonly IGoogleMusicSessionService sessionService;

        public SongWebService(
            ILogManager logManager,
            IGoogleMusicWebService googleMusicWebService,
            IGoogleMusicSessionService sessionService)
        {
            this.logger = logManager.CreateLogger("SongWebService");
            this.googleMusicWebService = googleMusicWebService;
            this.sessionService = sessionService;
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
                                                                                          sessionId = this.sessionService.GetSession().SessionId
                                                                                      }) }
                                        };

            var response = await this.googleMusicWebService.PostAsync(RecordPlayingUrl, formData: requestParameters);
            return response.StatusCode == HttpStatusCode.OK && (await response.Content.ReadAsJsonObject<SuccessResult>()).Success;
        }

        public async Task<RatingResp> UpdateRatingAsync(GoogleMusicSong song, int rating)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { 
                                                "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          entries = new[]
                                                                                                        {
                                                                                                            new { id = song.Id, rating = rating }
                                                                                                        },
                                                                                          sessionId = this.sessionService.GetSession().SessionId
                                                                                      }) 
                                            }
                                        };

            var response = await this.googleMusicWebService.PostAsync(ModifyEntriesUrl, formData: requestParameters);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadAsJsonObject<RatingResp>();
            }
            else
            {
                return null;
            }
        }
    }
}