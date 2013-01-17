// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class GoogleAccountWebService : IGoogleAccountWebService
    {
        private const string ClientLoginPath = "accounts/ClientLogin";
        private const string IssueAuthTokenPath = "accounts/IssueAuthToken";
        private const string TokenAuthPath = "accounts/TokenAuth";

        private readonly ILogger logger;
        private readonly HttpClientHandler httpClientHandler;
        private readonly HttpClient httpClient;

        private string token;

        public GoogleAccountWebService(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("GoogleAccountWebService");

            this.httpClientHandler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };

            this.httpClient = new HttpClient(this.httpClientHandler)
                                  {
                                      BaseAddress = new Uri("https://www.google.com"),
                                      Timeout = TimeSpan.FromSeconds(10)
                                  };

            this.httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Music Manager (1, 0, 54, 4672 - Windows)");
        }

        public async Task<GoogleLoginResponse> Authenticate(string email, string password)
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

            var responseMessage = await this.httpClient.SendAsync(
                                                        new HttpRequestMessage(HttpMethod.Post, ClientLoginPath)
                                                            {
                                                                Content = new FormUrlEncodedContent(requestParameters)
                                                            },
                                                        HttpCompletionOption.ResponseContentRead);

            if (this.logger.IsDebugEnabled)
            {
                await this.logger.LogResponseAsync(ClientLoginPath, responseMessage);
            }

            if (!responseMessage.Content.IsPlainText())
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

            if (responseMessage.IsSuccessStatusCode && !string.IsNullOrEmpty(dictionary["SID"]) && !string.IsNullOrEmpty(dictionary["LSID"]))
            {
                var authResponseMessage = await this.httpClient.SendAsync(
                                                        new HttpRequestMessage(HttpMethod.Post, IssueAuthTokenPath)
                                                            {
                                                                Content = new FormUrlEncodedContent(
                                                                    new Dictionary<string, string>() { { "SID", dictionary["SID"] }, { "LSID", dictionary["LSID"] }, { "service", "gaia" } })
                                                            },
                                                        HttpCompletionOption.ResponseContentRead);

                if (this.logger.IsDebugEnabled)
                {
                    await this.logger.LogResponseAsync(IssueAuthTokenPath, authResponseMessage);
                }

                if (authResponseMessage.IsSuccessStatusCode)
                {
                    this.token = await authResponseMessage.Content.ReadAsStringAsync();

                    return GoogleLoginResponse.SuccessResponse();
                }
            }

            GoogleLoginResponse.ErrorResponseCode error = GoogleLoginResponse.ErrorResponseCode.Unknown;

            string dictionaryValue;
            if (dictionary.TryGetValue("Error", out dictionaryValue))
            {
                Enum.TryParse(dictionary["Error"], out error);
            }

            return GoogleLoginResponse.ErrorResponse(error);
        }

        public async Task<CookieCollection> GetCookiesAsync(string redirectUrl)
        {
            var url = new StringBuilder(TokenAuthPath);
            url.Append("?");
            url.AppendFormat("auth={0}", WebUtility.UrlEncode(this.token));
            url.AppendFormat("&service=sj");
            url.AppendFormat("&continue={0}", WebUtility.UrlEncode(redirectUrl));
            var requestUri = url.ToString();

            var responseMessage = await this.httpClient.GetAsync(requestUri);

            if (this.logger.IsDebugEnabled)
            {
                await this.logger.LogResponseAsync(TokenAuthPath, responseMessage);
            }

            if (responseMessage.IsSuccessStatusCode 
                && string.Equals(responseMessage.RequestMessage.RequestUri.Host, new Uri(redirectUrl).Host, StringComparison.OrdinalIgnoreCase))
            {
                return this.httpClientHandler.CookieContainer.GetCookies(new Uri(redirectUrl));
            }

            return null;
        }
    }
}
