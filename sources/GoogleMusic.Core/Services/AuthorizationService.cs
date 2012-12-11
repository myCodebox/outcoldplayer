// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.WebServices;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class AuthorizationService
    {
        private readonly IClientLoginService clientLoginWebService;
        private readonly IUserAuthorizationDataService userAuthorizationDataService;
        private readonly ICookieManager cookieManager;
        private readonly ILogger logger;

        public AuthorizationService(
            ILogManager logManager,
            IClientLoginService clientLoginWebService,
            IUserAuthorizationDataService userAuthorizationDataService,
            ICookieManager cookieManager)
        {
            this.logger = logManager.CreateLogger("AuthorizationService");
            this.clientLoginWebService = clientLoginWebService;
            this.userAuthorizationDataService = userAuthorizationDataService;
            this.cookieManager = cookieManager;
        }

        public async Task AuthorizeAsync()
        {
            GoogleWebResponse loginResponse = null;

            do
            {
                // TODO: implement captcha
                var userInfo = await this.userAuthorizationDataService.GetUserSecurityDataAsync();
                loginResponse = await this.clientLoginWebService.LoginAsync(userInfo.Email, userInfo.Password);

                // TODO:  Show error
            }
            while (loginResponse.HttpWebResponse.StatusCode != HttpStatusCode.OK);

            string auth = loginResponse.GetAsPlainLines()
                .Where(x => x.Key.Equals("Auth", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Value)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(auth))
            {
                this.logger.Error("Cannot get auth from body.");
                throw new InvalidDataException("Cannot get auth from body");
            }

            var cookieResponse = await this.clientLoginWebService.GetCookieAsync(auth);

            this.cookieManager.SaveCookies(cookieResponse.HttpWebResponse.ResponseUri, cookieResponse.HttpWebResponse.Cookies);
        }
    }
}