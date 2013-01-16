// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    using Windows.ApplicationModel.Resources;

    public class AuthentificationService : IAuthentificationService
    {
        private readonly ILogger logger;
        private readonly IGoogleAccountService googleAccountService;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly IGoogleAccountWebService googleAccountWebService;
        private readonly IGoogleMusicWebService googleMusicWebService;
        private readonly IPlaylistsWebService playlistsWebService;

        private readonly ResourceLoader resourceLoader = new ResourceLoader("CoreResources");

        public AuthentificationService(
            ILogManager logManager,
            IGoogleAccountService googleAccountService,
            IGoogleMusicSessionService sessionService,
            IGoogleAccountWebService googleAccountWebService,
            IGoogleMusicWebService googleMusicWebService,
            IPlaylistsWebService playlistsWebService)
        {
            this.logger = logManager.CreateLogger("AuthentificationService");
            this.googleAccountService = googleAccountService;
            this.sessionService = sessionService;
            this.googleAccountWebService = googleAccountWebService;
            this.googleMusicWebService = googleMusicWebService;
            this.playlistsWebService = playlistsWebService;
        }

        public async Task<AuthentificationResult> CheckAuthentificationAsync(UserInfo userInfo = null)
        {
            if (userInfo == null)
            {
                this.logger.Debug("CheckAuthentificationAsync: trying to restore previos session.");
                var userSession = this.sessionService.GetSession();
                if (userSession != null)
                {
                    this.logger.Debug("CheckAuthentificationAsync: GetSession is not null.");

                    var cookieCollection = await this.sessionService.GetSavedCookiesAsync();
                    if (cookieCollection != null)
                    {
                        this.logger.Debug("CheckAuthentificationAsync: cookie collection is not null. Initializing web services.");
                        if (await this.InitializeWebServices(cookieCollection))
                        {
                            userSession.IsAuthenticated = true;
                            return AuthentificationResult.SucceedResult();
                        }
                    }
                }
            }

            if (userInfo == null)
            {
                this.logger.Debug("CheckAuthentificationAsync: Trying to get user info with pasword.");
                userInfo = this.googleAccountService.GetUserInfo(retrievePassword: true);
            }

            if (userInfo == null)
            {
                this.logger.Debug("CheckAuthentificationAsync: Cannot get user info.");
                return AuthentificationResult.FailedResult(null);
            }

            this.logger.Debug("Logging.");
            GoogleLoginResponse loginResponse = await this.googleAccountWebService.Authenticate(userInfo.Email, userInfo.Password);

            if (loginResponse.Success)
            {
                var cookieCollection = await this.googleAccountWebService.GetCookiesAsync(this.googleMusicWebService.GetServiceUrl());

                if (cookieCollection != null && cookieCollection.Count > 0)
                {
                    if (await this.InitializeWebServices(cookieCollection))
                    {
                        this.sessionService.GetSession().IsAuthenticated = true;
                        return AuthentificationResult.SucceedResult();
                    }
                }
            }
            else if (loginResponse.Error.HasValue)
            {
                string errorMessage = this.GetErrorMessage(loginResponse.Error.Value);
                this.logger.Warning("CheckAuthentificationAsync: ErrorMessage: {0}, error code: {1}", errorMessage, loginResponse.Error.Value);
                return AuthentificationResult.FailedResult(errorMessage);
            }

            this.logger.Error("CheckAuthentificationAsync: showing 'Login_Unknown'.");
            return AuthentificationResult.FailedResult(this.resourceLoader.GetString("Login_Unknown"));
        }

        private async Task<bool> InitializeWebServices(CookieCollection cookieCollection)
        {
            this.googleMusicWebService.Initialize(cookieCollection);

            var statusResp = await this.playlistsWebService.GetStatusAsync();
            if (statusResp != null 
                && string.IsNullOrEmpty(statusResp.ReloadXsrf)
                && (string.IsNullOrEmpty(statusResp.Success) || string.Equals(statusResp.Success, bool.TrueString, StringComparison.OrdinalIgnoreCase)))
            {
                this.logger.Debug("InitializeWebServices: GetStatusAsync returns for us success result.");
                return true;
            }

            this.logger.Debug("InitializeWebServices: GetStatusAsync returns for us unsuccess result.");

            return false;
        }

        private string GetErrorMessage(GoogleLoginResponse.ErrorResponseCode errorResponseCode)
        {
            switch (errorResponseCode)
            {
                case GoogleLoginResponse.ErrorResponseCode.BadAuthentication:
                    return this.resourceLoader.GetString("Login_BadAuthentication");
                case GoogleLoginResponse.ErrorResponseCode.NotVerified:
                    return this.resourceLoader.GetString("Login_NotVerified");
                case GoogleLoginResponse.ErrorResponseCode.TermsNotAgreed:
                    return this.resourceLoader.GetString("Login_TermsNotAgreed");
                case GoogleLoginResponse.ErrorResponseCode.CaptchaRequired:
                    return this.resourceLoader.GetString("Login_CaptchaRequired");
                case GoogleLoginResponse.ErrorResponseCode.Unknown:
                    return this.resourceLoader.GetString("Login_Unknown");
                case GoogleLoginResponse.ErrorResponseCode.AccountDeleted:
                    return this.resourceLoader.GetString("Login_AccountDeleted");
                case GoogleLoginResponse.ErrorResponseCode.AccountDisabled:
                    return this.resourceLoader.GetString("Login_AccountDisabled");
                case GoogleLoginResponse.ErrorResponseCode.ServiceDisabled:
                    return this.resourceLoader.GetString("Login_ServiceDisabled");
                case GoogleLoginResponse.ErrorResponseCode.ServiceUnavailable:
                    return this.resourceLoader.GetString("Login_ServiceUnavailable");
                default:
                    throw new NotSupportedException("Value is not supported: " + errorResponseCode.ToString());
            }
        }

        public class Captcha
        {
            public Captcha(string captchaToken, string captchaUrl)
            {
                this.CaptchaToken = captchaToken;
                this.CaptchaUrl = captchaUrl;
            }

            public string CaptchaToken { get; private set; }

            public string CaptchaUrl { get; private set; }
        }

        public class AuthentificationResult
        {
            private AuthentificationResult(bool succeed, string errorMessage = null)
            {
                this.Succeed = succeed;
                this.ErrorMessage = errorMessage;
            }

            public bool Succeed { get; private set; }

            public string ErrorMessage { get; set; }

            public static AuthentificationResult FailedResult(string errorMessage)
            {
                return new AuthentificationResult(succeed: false, errorMessage: errorMessage);
            }

            public static AuthentificationResult SucceedResult()
            {
                return new AuthentificationResult(succeed: true);
            }
        }
    }
}
