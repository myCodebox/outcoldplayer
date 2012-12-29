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
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class SongWebService : ISongWebService
    {
        private const string SongUrlFormat = "https://play.google.com/music/play?u=0&songid={0}&pt=e";
        private const string RecordPlayingUrl = "https://play.google.com/music/services/recordplaying";

        private readonly ILogger logger;
        private readonly IGoogleWebService googleWebService;

        public SongWebService(
            ILogManager logManager,
            IGoogleWebService googleWebService)
        {
            this.logger = logManager.CreateLogger("SongWebService");
            this.googleWebService = googleWebService;
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
                this.logger.Error("Cannot get url for song by url '{0}'.", url);
                return null;
            }
        }

        public async Task<bool> RecordPlayingAsync(string id, int playCounts)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { "json", JsonConvert.SerializeObject(new { songId = id, playCount = playCounts }) }
                                        };

            var response = await this.googleWebService.PostAsync(RecordPlayingUrl, arguments: requestParameters);

            return response.GetAsJsonObject<SuccessResult>().Success;
        }
    }
}