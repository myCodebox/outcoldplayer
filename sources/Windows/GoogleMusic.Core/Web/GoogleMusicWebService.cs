// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;

    public class GoogleMusicWebService : IGoogleMusicWebService
    {
        private const string OriginUrl = "https://play.google.com";
        private const string PlayMusicUrl = "https://play.google.com/music/listen?u=0&hl=en";

        private readonly ILogger logger;
        private readonly IGoogleMusicSessionService sessionService;

        private HttpClient httpClient;
        private HttpClientHandler httpClientHandler;

        public GoogleMusicWebService(
            ILogManager logManager,
            IGoogleMusicSessionService sessionService)
        {
            this.sessionService = sessionService;
            this.sessionService.SessionCleared += (sender, args) =>
                {
                    this.httpClientHandler = null;
                    this.httpClient = null;
                };

            this.logger = logManager.CreateLogger("GoogleAccountWebService");
        }

        public string GetServiceUrl()
        {
            return PlayMusicUrl;
        }

        public void Initialize(CookieCollection cookieCollection)
        {
            if (cookieCollection == null)
            {
                throw new ArgumentNullException("cookieCollection");
            }

            this.httpClientHandler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };

            this.httpClient = new HttpClient(this.httpClientHandler)
                                  {
                                      BaseAddress = new Uri(OriginUrl),
                                      Timeout = TimeSpan.FromSeconds(10)
                                  };

            this.httpClientHandler.CookieContainer.Add(new Uri(this.GetServiceUrl()), cookieCollection);
        }

        public CookieCollection GetCurrentCookies()
        {
            return this.httpClientHandler.CookieContainer.GetCookies(new Uri(this.GetServiceUrl()));
        }

        public async Task<HttpResponseMessage> GetAsync(
            string url,
            bool authenticated = true)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.LogRequest(HttpMethod.Get, url, this.httpClientHandler.CookieContainer.GetCookies(new Uri(PlayMusicUrl)));
            }

            if (authenticated)
            {
                url = this.SignUrl(url);
            }

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            var responseMessage = await this.httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);

            if (this.logger.IsDebugEnabled)
            {
                await this.logger.LogResponseAsync(url, responseMessage);
            }

            this.VerifyAuthorization(responseMessage);

            return responseMessage;
        }

        public async Task<HttpResponseMessage> PostAsync(
            string url,
            IDictionary<string, string> formData = null, 
            bool authenticated = true)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.LogRequest(HttpMethod.Post, url, this.httpClientHandler.CookieContainer.GetCookies(new Uri(PlayMusicUrl)), formData);
            }

            if (authenticated)
            {
                url = this.SignUrl(url);
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

            if (formData != null)
            {
                requestMessage.Content = new FormUrlEncodedContent(formData);
            }

            var responseMessage = await this.httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);

            if (this.logger.IsDebugEnabled)
            {
                await this.logger.LogResponseAsync(url, responseMessage);
            }

            this.VerifyAuthorization(responseMessage);

            return responseMessage;
        }

        private string SignUrl(string url)
        {
            var cookieCollection = this.httpClientHandler.CookieContainer.GetCookies(new Uri(PlayMusicUrl));

            var cookie = cookieCollection.Cast<Cookie>().FirstOrDefault(x => string.Equals(x.Name, "xt", StringComparison.OrdinalIgnoreCase));
            if (cookie != null)
            {
                if (url.IndexOf("?", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    url += "?";
                }
                else
                {
                    url += "&";
                }

                url += string.Format(CultureInfo.InvariantCulture, "u=0&xt={0}", cookie.Value);

                this.logger.Debug("Found XT cookie, transforming url to '{0}'.", url);
            }
            else
            {
                this.logger.Debug("Could not find XT cookie.");
            }

            return url;
        }

        private void VerifyAuthorization(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    this.logger.Warning("Got the Unauthorized http status code. Going to clear session.");

                    this.sessionService.ClearSession();
                    responseMessage.EnsureSuccessStatusCode();
                }
            }
        }
    }
}