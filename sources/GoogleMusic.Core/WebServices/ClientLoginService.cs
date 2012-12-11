// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class ClientLoginService : IClientLoginService
    {
        private const string ClientLoginUrl = "https://www.google.com/accounts/ClientLogin";
        private const string GetAuthCookie = "https://play.google.com/music/listen?hl=en&u=0";

        private readonly ILogger logger;
        private readonly IGoogleWebService googleWebService;

        public ClientLoginService(ILogManager logManager, IGoogleWebService googleWebService)
        {
            this.googleWebService = googleWebService;
            this.logger = logManager.CreateLogger("ClientLoginService");
        }

        public async Task<GoogleWebResponse> LoginAsync(string email, string password)
        {
            // Check for details: https://developers.google.com/accounts/docs/AuthForInstalledApps
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { "accountType", "HOSTED_OR_GOOGLE" },
                                            { "Email", email },
                                            { "Passwd", password },
                                            { "service", "sj" }
                                        };

            return await this.googleWebService.PostAsync(ClientLoginUrl, arguments: requestParameters);
        }

        public async Task<GoogleWebResponse> GetCookieAsync(string auth)
        {
            var requestParameters = new Dictionary<HttpRequestHeader, string>
                                        {
                                            { HttpRequestHeader.Authorization, string.Format(CultureInfo.InvariantCulture, "GoogleLogin auth={0}", auth) }
                                        };

            return await this.googleWebService.PostAsync(GetAuthCookie, requestParameters);
        }
    }
}
