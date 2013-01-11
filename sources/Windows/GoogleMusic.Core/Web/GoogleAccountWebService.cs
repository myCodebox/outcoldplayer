// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class GoogleAccountWebService : IGoogleAccountWebService
    {
        private const string ClientLoginPath = "accounts/ClientLogin";
        private const string IssueAuthTokenPath = "accounts/IssueAuthToken";
        private const string TokenAuthPath = "accounts/TokenAuth";

        private readonly ILogger logger;
        
        private readonly HttpClient client = new HttpClient()
                                                 {
                                                     BaseAddress = new Uri("https://www.google.com")
                                                 };

        public GoogleAccountWebService(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("GoogleAccountWebService");

            this.client.DefaultRequestHeaders.UserAgent.ParseAdd("Music Manager (1, 0, 24, 7712 - Windows)");
        }

        public async Task<GoogleLoginResponse> LoginAsync(string email, string password)
        {
            this.logger.Debug("Calling ClientLogin.");

            // Check for details: https://developers.google.com/accounts/docs/AuthForInstalledApps
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { "accountType", "HOSTED_OR_GOOGLE" },
                                            { "Email", email },
                                            { "Passwd", password },
                                            { "service", "sj" }
                                        };

            var responseMessage = await this.client.PostAsync(ClientLoginPath, new FormUrlEncodedContent(requestParameters));

            if (!responseMessage.Content.Headers.ContentType.IsPlainText())
            {
                if (this.logger.IsErrorEnabled)
                {
                    this.logger.Error(
                        "ClientLogin content response is not a text/plain, it is '{0}'.",
                        responseMessage.Content.Headers.ContentType);
                }

                return GoogleLoginResponse.ErrorResponse(GoogleLoginResponse.ErrorResponseCode.Unknown);
            }

            var dictionary = await responseMessage.Content.ReadAsDictionaryAsync();

            string auth = null;
            string sid = null;
            string lsid = null;
            string captchaToken = null;
            string captchaUrl = null;
            GoogleLoginResponse.ErrorResponseCode error = GoogleLoginResponse.ErrorResponseCode.Unknown;

            foreach (var line in dictionary)
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("ClientLogin response line: Key='{0}', Value='{1}'.", line.Key, line.Value);
                }

                if (string.Equals(line.Key, "Auth", StringComparison.OrdinalIgnoreCase))
                {
                    auth = line.Value;
                }
                else if (string.Equals(line.Key, "SID", StringComparison.OrdinalIgnoreCase))
                {
                    sid = line.Value;
                }
                else if (string.Equals(line.Key, "LSID", StringComparison.OrdinalIgnoreCase))
                {
                    lsid = line.Value;
                }
                else if (string.Equals(line.Key, "Error", StringComparison.OrdinalIgnoreCase))
                {
                    Enum.TryParse(line.Value, out error);
                }
                else if (string.Equals(line.Key, "CaptchaToken", StringComparison.OrdinalIgnoreCase))
                {
                    captchaToken = line.Value;
                }
                else if (string.Equals(line.Key, "CaptchaUrl", StringComparison.OrdinalIgnoreCase))
                {
                    captchaUrl = line.Value;
                }
            }

            if (responseMessage.IsSuccessStatusCode && !string.IsNullOrEmpty(auth))
            {
                var authResponseMessage = await this.client.PostAsync(
                                                    IssueAuthTokenPath,
                                                    new FormUrlEncodedContent(
                                                    new Dictionary<string, string>() { { "SID", sid }, { "LSID", lsid }, { "service", "gaia" } }));
                if (authResponseMessage.IsSuccessStatusCode)
                {
                    return GoogleLoginResponse.SuccessResponse(await authResponseMessage.Content.ReadAsStringAsync());
                }
            }

            return GoogleLoginResponse.ErrorResponse(error, captchaToken, captchaUrl);
        }
    }
}
