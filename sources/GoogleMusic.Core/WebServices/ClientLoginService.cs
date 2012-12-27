// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class ClientLoginService : IClientLoginService
    {
        private const string ClientLoginUrl = "https://www.google.com/accounts/ClientLogin";
        private const string GetAuthCookie = "https://play.google.com/music/listen?hl=en";

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

        public async Task<GoogleWebResponse> GetCookieAsync(string auth)
        {
            this.logger.Debug("GetCookieAsync");

            return await this.googleWebService.PostAsync(GetAuthCookie);
        }
    }
}
