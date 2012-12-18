// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System.Net;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class SongWebService : ISongWebService
    {
        private const string SongUrlFormat = "https://play.google.com/music/play?u=0&songid={0}&pt=e";

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
    }
}