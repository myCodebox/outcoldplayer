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

    public class SongWebService : WebServiceBase, ISongWebService
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
            : base(googleMusicWebService)
        {
            this.logger = logManager.CreateLogger("SongWebService");
            this.googleMusicWebService = googleMusicWebService;
            this.sessionService = sessionService;
        }

        public async Task<GoogleMusicSongUrl> GetSongUrlAsync(string id)
        {
            var url = string.Format(SongUrlFormat, id);
            var response = await this.googleMusicWebService.GetAsync(url, authenticated: false);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    var googleMusicSongUrl = await response.Content.ReadAsJsonObject<GoogleMusicSongUrl>();

                    if (await this.NeedRetry(googleMusicSongUrl))
                    {
                        response = await this.googleMusicWebService.GetAsync(url, authenticated: false);
                        googleMusicSongUrl = await response.Content.ReadAsJsonObject<GoogleMusicSongUrl>();
                    }

                    return googleMusicSongUrl;
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

        public async Task<bool> RecordPlayingAsync(GoogleMusicSong song, string playlistId, bool updateRecentAlbum, bool updateRecentPlaylist, int playCount)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          songId = song.Id, 
                                                                                          playCount, 
                                                                                          updateRecentAlbum,
                                                                                          updateRecentPlaylist,
                                                                                          playlistId,
                                                                                          sessionId = this.sessionService.GetSession().SessionId
                                                                                      }) }
                                        };

            var response = await this.googleMusicWebService.PostAsync(RecordPlayingUrl, formData: requestParameters);
            var commonResponse = await response.Content.ReadAsJsonObject<CommonResponse>();

            if (await this.NeedRetry(commonResponse))
            {
                response = await this.googleMusicWebService.PostAsync(RecordPlayingUrl, formData: requestParameters);
                commonResponse = await response.Content.ReadAsJsonObject<CommonResponse>();
            }

            return response.StatusCode == HttpStatusCode.OK && commonResponse.Success.HasValue && commonResponse.Success.Value;
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
                var ratingResp = await response.Content.ReadAsJsonObject<RatingResp>();
                if (await this.NeedRetry(ratingResp))
                {
                    response = await this.googleMusicWebService.PostAsync(ModifyEntriesUrl, formData: requestParameters);
                    ratingResp = await response.Content.ReadAsJsonObject<RatingResp>();
                }

                return ratingResp;
            }
            else
            {
                return null;
            }
        }
    }
}