// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    using Windows.ApplicationModel.Resources;

    public class AuthentificationService : IAuthentificationService
    {
        private readonly ILogger logger;
        private readonly IUserDataStorage userDataStorage;
        private readonly IGoogleAccountWebService googleAccountWebService;
        private readonly IGoogleMusicWebService googleMusicWebService;

        private readonly ResourceLoader resourceLoader = new ResourceLoader("CoreResources");

        public AuthentificationService(
            ILogManager logManager,
            IUserDataStorage userDataStorage,
            IGoogleAccountWebService googleAccountWebService,
            IGoogleMusicWebService googleMusicWebService)
        {
            this.logger = logManager.CreateLogger("AuthentificationService");
            this.userDataStorage = userDataStorage;
            this.googleAccountWebService = googleAccountWebService;
            this.googleMusicWebService = googleMusicWebService;
        }

        public async Task<AuthentificationResult> CheckAuthentificationAsync(UserInfo userInfo = null)
        {
            if (userInfo == null)
            {
                this.logger.Debug("Trying to get user info.");
                userInfo = this.userDataStorage.GetUserInfo();
            }

            if (userInfo == null)
            {
                this.logger.Debug("Cannot get user info.");
                return AuthentificationResult.FailedResult(null);
            }

            this.logger.Debug("Logging.");
            GoogleLoginResponse loginResponse = await this.googleAccountWebService.Authenticate(userInfo.Email, userInfo.Password);

            if (loginResponse.Success)
            {
                var cookieCollection = await this.googleAccountWebService.GetCookiesAsync(this.googleMusicWebService.GetServiceUrl());

                if (cookieCollection != null && cookieCollection.Count > 0)
                {
                    this.googleMusicWebService.Initialize(cookieCollection);

                    this.userDataStorage.SetUserSession(new UserSession());

                    return AuthentificationResult.SucceedResult();
                }
                else
                {
                    this.logger.Error("Cannot get cookie. Web Response Status code is '{0}'.", false);

                    // Better error
                    return AuthentificationResult.FailedResult(this.resourceLoader.GetString("Login_Unknown"));
                }
            }
            else
            {
                this.logger.Warning("Could not log in.");

                string errorMessage = this.GetErrorMessage(loginResponse.Error.Value);

                this.logger.Warning("ErrorMessage: {0}, error code: {1}", errorMessage, loginResponse.Error.Value);
                return AuthentificationResult.FailedResult(errorMessage);
            }
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
