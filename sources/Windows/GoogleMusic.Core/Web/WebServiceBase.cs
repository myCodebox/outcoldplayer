// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public abstract class WebServiceBase
    {
        private readonly IGoogleAccountWebService googleAccountWebService;
        private readonly IGoogleAccountService googleAccountService;
        private readonly IGoogleMusicWebService googleMusicWebService;
        private readonly IGoogleMusicSessionService sessionService;

        protected WebServiceBase(
            IGoogleAccountWebService googleAccountWebService,
            IGoogleAccountService googleAccountService,
            IGoogleMusicWebService googleMusicWebService,
            IGoogleMusicSessionService sessionService)
        {
            this.googleAccountWebService = googleAccountWebService;
            this.googleAccountService = googleAccountService;
            this.googleMusicWebService = googleMusicWebService;
            this.sessionService = sessionService;
        }

        protected async Task<bool> NeedRetry(CommonResponse commonResponse)
        {
            if (commonResponse.ReloadXsrf.HasValue && commonResponse.ReloadXsrf.Value)
            {
                var userInfo = this.googleAccountService.GetUserInfo(retrievePassword: true);
                if (userInfo != null)
                {
                    var googleLoginResponse = await this.googleAccountWebService.AuthenticateAsync(userInfo.Email, userInfo.Password);
                    if (googleLoginResponse.Success)
                    {
                        var cookies = await this.googleAccountWebService.GetCookiesAsync(this.googleMusicWebService.GetServiceUrl());
                        if (cookies != null)
                        {
                            this.googleMusicWebService.Initialize(cookies);
                            return true;
                        }
                    }
                }

                this.sessionService.ClearSession();
            }

            return false;
        }
    }
}