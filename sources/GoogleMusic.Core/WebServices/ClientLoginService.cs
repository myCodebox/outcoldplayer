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
        private const string GetAuthCookie = "https://play.google.com/music/listen?hl=en";
        private const string GetStatusUrl = "https://play.google.com/music/services/getstatus";

        private readonly ILogger logger;
        private readonly IGoogleWebService googleWebService;

        public ClientLoginService(ILogManager logManager, IGoogleWebService googleWebService)
        {
            this.googleWebService = googleWebService;
            this.logger = logManager.CreateLogger("ClientLoginService");
        }

        public async Task<GoogleLoginResponse> LoginAsync(string email, string password)
        {
            this.logger.Debug("LoginAsync");

            // Check for details: https://developers.google.com/accounts/docs/AuthForInstalledApps
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { "accountType", "HOSTED_OR_GOOGLE" },
                                            { "Email", email },
                                            { "Passwd", password },
                                            { "service", "sj" }
                                        };

            return new GoogleLoginResponse(await this.googleWebService.PostAsync(ClientLoginUrl, arguments: requestParameters));
        }

        public async Task<GoogleWebResponse> GetCookieAsync(string auth = null)
        {
            this.logger.Debug("GetCookieAsync");

            Dictionary<HttpRequestHeader, string> headers = new Dictionary<HttpRequestHeader, string>();

            if (!string.IsNullOrEmpty(auth))
            {
                headers.Add(HttpRequestHeader.Authorization, string.Format(CultureInfo.InvariantCulture, "GoogleLogin auth={0}", auth));
            }

            return await this.googleWebService.PostAsync(GetAuthCookie, headers: headers);
        }

        public async Task<StatusResp> GetStatusAsync()
        {
            var googleWebResponse = await this.googleWebService.PostAsync(GetStatusUrl);

            if (googleWebResponse.HttpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                return googleWebResponse.GetAsJsonObject<StatusResp>();
            }
            else
            {
                return null;
            }
        }
    }
}
