// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public abstract class WebServiceBase
    {
        private const string RefreshXtUrl = "music/refreshxt";

        private readonly IGoogleMusicWebService googleMusicWebService;

        protected WebServiceBase(
            IGoogleMusicWebService googleMusicWebService)
        {
            this.googleMusicWebService = googleMusicWebService;
        }

        protected async Task<bool> NeedRetry(CommonResponse commonResponse)
        {
            if (commonResponse.ReloadXsrf.HasValue && commonResponse.ReloadXsrf.Value)
            {
                await this.googleMusicWebService.PostAsync(RefreshXtUrl);
                return true;
            }

            return false;
        }
    }
}