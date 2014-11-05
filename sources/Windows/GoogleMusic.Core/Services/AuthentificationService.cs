// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Text;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class AuthentificationService : IAuthentificationService
    {
        private readonly ILogger logger;

        private readonly IApplicationResources resources;
        private readonly IGoogleAccountService googleAccountService;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly IGoogleAccountWebService googleAccountWebService;
        private readonly IGoogleMusicWebService googleMusicWebService;
        private readonly IConfigWebService configWebService;

        public AuthentificationService(
            IApplicationResources resources,
            ILogManager logManager,
            IGoogleAccountService googleAccountService,
            IGoogleMusicSessionService sessionService,
            IGoogleAccountWebService googleAccountWebService,
            IGoogleMusicWebService googleMusicWebService,
            IConfigWebService configWebService)
        {
            this.logger = logManager.CreateLogger("AuthentificationService");
            this.resources = resources;
            this.googleAccountService = googleAccountService;
            this.sessionService = sessionService;
            this.googleAccountWebService = googleAccountWebService;
            this.googleMusicWebService = googleMusicWebService;
            this.configWebService = configWebService;
        }

        public async Task<AuthentificationResult> CheckAuthentificationAsync(UserInfo userInfo = null, string captchaToken = null, string captcha = null, bool forceCaptcha = false)
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
                        this.sessionService.InitializeCookieContainer(cookieCollection, this.sessionService.GetAuth());
                        this.logger.Debug("CheckAuthentificationAsync: cookie collection is not null. Initializing.");
                        userSession.IsAuthenticated = true; 
                        return AuthentificationResult.SucceedResult();
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

#if DEBUG
            if (forceCaptcha && string.IsNullOrEmpty(captchaToken))
            {
                while (true)
                {
                    GoogleAuthResponse response = await this.googleAccountWebService.AuthenticateAsync(
                        new Uri(this.googleMusicWebService.GetServiceUrl()), userInfo.Email, RandomString(), null, null);
                    if (response.Error == GoogleAuthResponse.ErrorResponseCode.CaptchaRequired)
                    {
                        var result = AuthentificationResult.FailedResult(this.GetErrorMessage(response.Error.Value));
                        result.IsCaptchaRequired = true;
                        result.CaptchaUrl = response.CaptchaUrl;
                        result.CaptchaToken = response.CaptchaToken;
                        return result;
                    }
                }
            }
#endif

            GoogleAuthResponse authResponse = await this.googleAccountWebService.AuthenticateAsync(
                new Uri(this.googleMusicWebService.GetServiceUrl()), userInfo.Email, userInfo.Password, captchaToken, captcha);

            if (authResponse.Success)
            {
                if (authResponse.CookieCollection != null && authResponse.CookieCollection.Count > 0)
                {
                    this.sessionService.InitializeCookieContainer(authResponse.CookieCollection.Cast<Cookie>(), authResponse.Auth);

                    if (!await this.configWebService.IsAccesptedUserAsync())
                    {
                        await this.sessionService.ClearSession(silent: true);
                        return AuthentificationResult.FailedResult(this.GetErrorMessage(GoogleAuthResponse.ErrorResponseCode.TermsNotAgreed));
                    }

                    await this.sessionService.SaveCurrentSessionAsync();
                    this.sessionService.GetSession().IsAuthenticated = true;
                    return AuthentificationResult.SucceedResult();
                }
            }
            else if (authResponse.Error.HasValue)
            {
                string errorMessage = this.GetErrorMessage(authResponse.Error.Value);
                this.logger.Warning("CheckAuthentificationAsync: ErrorMessage: {0}, error code: {1}", errorMessage, authResponse.Error.Value);
                var result = AuthentificationResult.FailedResult(errorMessage);

                if (authResponse.Error.Value == GoogleAuthResponse.ErrorResponseCode.CaptchaRequired)
                {
                    result.IsCaptchaRequired = true;
                    result.CaptchaUrl = authResponse.CaptchaUrl;
                    result.CaptchaToken = authResponse.CaptchaToken;
                }

                return result;
            }

            this.logger.Error("CheckAuthentificationAsync: showing 'Login_Unknown'.");
            return AuthentificationResult.FailedResult(this.resources.GetString("Authorization_Error_Unknown"));
        }

        private string GetErrorMessage(GoogleAuthResponse.ErrorResponseCode errorResponseCode)
        {
            switch (errorResponseCode)
            {
                case GoogleAuthResponse.ErrorResponseCode.BadAuthentication:
                    return this.resources.GetString("Authorization_Error_BadAuthentication");
                case GoogleAuthResponse.ErrorResponseCode.NotVerified:
                    return this.resources.GetString("Authorization_Error_NotVerified");
                case GoogleAuthResponse.ErrorResponseCode.TermsNotAgreed:
                    return this.resources.GetString("Authorization_Error_TermsNotAgreed");
                case GoogleAuthResponse.ErrorResponseCode.CaptchaRequired:
                    return this.resources.GetString("Authorization_Error_CaptchaRequired");
                case GoogleAuthResponse.ErrorResponseCode.Unknown:
                    return this.resources.GetString("Authorization_Error_Unknown");
                case GoogleAuthResponse.ErrorResponseCode.AccountDeleted:
                    return this.resources.GetString("Authorization_Error_AccountDeleted");
                case GoogleAuthResponse.ErrorResponseCode.AccountDisabled:
                    return this.resources.GetString("Authorization_Error_AccountDisabled");
                case GoogleAuthResponse.ErrorResponseCode.ServiceDisabled:
                    return this.resources.GetString("Authorization_Error_ServiceDisabled");
                case GoogleAuthResponse.ErrorResponseCode.ServiceUnavailable:
                    return this.resources.GetString("Authorization_Error_ServiceUnavailable");
                default:
                    throw new NotSupportedException("Value is not supported: " + errorResponseCode.ToString());
            }
        }

#if DEBUG
        private static readonly Random Random = new Random((int)DateTime.Now.Ticks);
        private string RandomString()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < Random.Next(1, 12); i++)
            {
                char ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * Random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
#endif

        public class AuthentificationResult
        {
            private AuthentificationResult(bool succeed, string errorMessage = null)
            {
                this.Succeed = succeed;
                this.ErrorMessage = errorMessage;
            }

            public bool Succeed { get; private set; }

            public string ErrorMessage { get; set; }

            public bool IsCaptchaRequired { get; set; }

            public string CaptchaToken { get; set; }

            public string CaptchaUrl { get; set; }

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
