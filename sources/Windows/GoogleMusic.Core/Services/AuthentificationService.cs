// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.WebServices;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class AuthentificationService : IAuthentificationService
    {
        private readonly ILogger logger;
        private readonly IUserDataStorage userDataStorage;
        private readonly IClientLoginService clientLoginService;

        public AuthentificationService(
            ILogManager logManager,
            IUserDataStorage userDataStorage,
            IClientLoginService clientLoginService)
        {
            this.logger = logManager.CreateLogger("AuthentificationService");
            this.userDataStorage = userDataStorage;
            this.clientLoginService = clientLoginService;
        }

        public async Task<AuthentificationResult> CheckAuthentificationAsync(UserInfo userInfo = null)
        {
            var session = this.userDataStorage.GetUserSession();

            if (session != null && session.Cookies != null && session.Cookies.Length > 0)
            {
                this.logger.Debug("User session is not null. Checking authentification with exisiting cookies");
                var statusResp = await this.clientLoginService.GetStatusAsync();
                if (statusResp != null && string.IsNullOrEmpty(statusResp.Success) && string.IsNullOrEmpty(statusResp.ReloadXsrf))
                {
                    return AuthentificationResult.SucceedResult();
                }
            }

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
            GoogleLoginResponse loginResponse = await this.clientLoginService.LoginAsync(userInfo.Email, userInfo.Password);

            if (loginResponse.IsOk)
            {
                this.logger.Debug("Logged in.");

                string auth = loginResponse.GetAuth();

                this.logger.Debug("Getting cookies.");
                GoogleWebResponse cookieResponse = await this.clientLoginService.GetCookieAsync(auth);
                if (cookieResponse.HttpWebResponse.StatusCode == HttpStatusCode.OK
                    && !string.Equals(cookieResponse.HttpWebResponse.ResponseUri.Host, "accounts.google.com", StringComparison.OrdinalIgnoreCase))
                {
                    var cookies = GetCookies(cookieResponse);

                    this.logger.Debug("Cookies count: {0}", cookies.Length);

                    var userSession = new UserSession(auth)
                                          {
                                              Cookies = cookies
                                          };

                    this.userDataStorage.SetUserSession(userSession);
                    return AuthentificationResult.SucceedResult();
                }
                else
                {
                    this.logger.Error("Cannot get cookie. Web Response Status code is '{0}'.", cookieResponse.HttpWebResponse.StatusCode);

                    // Better error
                    return AuthentificationResult.FailedResult(CoreResources.Login_Unknown);
                }
            }
            else
            {
                this.logger.Warning("Could not log in.");

                var errorResponse = loginResponse.AsError();
                string errorMessage = this.GetErrorMessage(errorResponse.Code);

                this.logger.Warning("ErrorMessage: {0}, error code: {1}", errorMessage, errorResponse.Code);
                var authentificationResult = AuthentificationResult.FailedResult(errorMessage);

                if (errorResponse.Code == GoogleLoginResponse.ErrorResponseCode.CaptchaRequired)
                {
                    authentificationResult.Captcha = new Captcha(errorResponse.CaptchaToken, errorResponse.CaptchaUrl);
                }

                return authentificationResult;
            }
        }

        private string GetErrorMessage(GoogleLoginResponse.ErrorResponseCode errorResponseCode)
        {
            switch (errorResponseCode)
            {
                case GoogleLoginResponse.ErrorResponseCode.BadAuthentication:
                    return CoreResources.Login_BadAuthentication;
                case GoogleLoginResponse.ErrorResponseCode.NotVerified:
                    return CoreResources.Login_NotVerified;
                case GoogleLoginResponse.ErrorResponseCode.TermsNotAgreed:
                    return CoreResources.Login_TermsNotAgreed;
                case GoogleLoginResponse.ErrorResponseCode.CaptchaRequired:
                    return CoreResources.Login_CaptchaRequired;
                case GoogleLoginResponse.ErrorResponseCode.Unknown:
                    return CoreResources.Login_Unknown;
                case GoogleLoginResponse.ErrorResponseCode.AccountDeleted:
                    return CoreResources.Login_AccountDeleted;
                case GoogleLoginResponse.ErrorResponseCode.AccountDisabled:
                    return CoreResources.Login_AccountDisabled;
                case GoogleLoginResponse.ErrorResponseCode.ServiceDisabled:
                    return CoreResources.Login_ServiceDisabled;
                case GoogleLoginResponse.ErrorResponseCode.ServiceUnavailable:
                    return CoreResources.Login_ServiceUnavailable;
                default:
                    throw new NotSupportedException("Value is not supported: " + errorResponseCode.ToString());
            }
        }

        private Cookie[] GetCookies(GoogleWebResponse response)
        {
            var cookieCollection = response.HttpWebResponse.Cookies;
            Cookie[] cookies = new Cookie[cookieCollection.Count];
            int index = 0;
            foreach (Cookie cookie in cookieCollection)
            {
                cookie.Path = string.Empty;
                cookies[index++] = cookie;
            }
            return cookies;
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

            public Captcha Captcha { get; set; }

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
