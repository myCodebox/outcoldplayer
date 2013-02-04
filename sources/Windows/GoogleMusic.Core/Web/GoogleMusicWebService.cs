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
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class GoogleMusicWebService : IGoogleMusicWebService
    {
        private const string OriginUrl = "https://play.google.com";
        private const string PlayMusicUrl = "https://play.google.com/music/listen?u=0&hl=en";
        private const string RefreshXtPath = "music/refreshxt";

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
                                      Timeout = TimeSpan.FromSeconds(30)
                                  };

            this.httpClientHandler.CookieContainer.Add(new Uri(this.GetServiceUrl()), cookieCollection);
        }

        public CookieCollection GetCurrentCookies()
        {
            return this.httpClientHandler.CookieContainer.GetCookies(new Uri(this.GetServiceUrl()));
        }

        public async Task<HttpResponseMessage> GetAsync(
            string url,
            bool signUrl = true)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.LogRequest(HttpMethod.Get, url, this.httpClientHandler.CookieContainer.GetCookies(new Uri(PlayMusicUrl)));
            }

            if (signUrl)
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
            bool signUrl = true)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.LogRequest(HttpMethod.Post, url, this.httpClientHandler.CookieContainer.GetCookies(new Uri(PlayMusicUrl)), formData);
            }

            if (signUrl)
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

        public async Task<TResult> GetAsync<TResult>(string url, bool signUrl = true) where TResult : CommonResponse
        {
            HttpResponseMessage responseMessage = await this.GetAsync(url, signUrl);
            TResult result = await responseMessage.Content.ReadAsJsonObject<TResult>();

            if (result.ReloadXsrf.HasValue && result.ReloadXsrf.Value)
            {
                this.logger.Debug("GetAsync :: Reload Xsrf requested. Reloading.");
                await this.PostAsync(RefreshXtPath);

                responseMessage = await this.GetAsync(url, signUrl);
                result = await responseMessage.Content.ReadAsJsonObject<TResult>();
            }

            return result;
        }

        public async Task<TResult> PostAsync<TResult>(
            string url, 
            IDictionary<string, string> formData = null, 
            IDictionary<string, string> jsonProperties = null,
            bool forceJsonBody = true,
            bool signUrl = true) where TResult : CommonResponse
        {
            if (forceJsonBody && (formData == null || !formData.ContainsKey("json")))
            {
                var jsonBody = new StringBuilder();

                jsonBody.Append("{");
                if (jsonProperties != null)
                {
                    foreach (var jsonProperty in jsonProperties)
                    {
                        jsonBody.AppendFormat("\"{0}\":{1},", jsonProperty.Key, jsonProperty.Value);
                    }
                }

                jsonBody.AppendFormat("\"sessionId\":{0}", JsonConvert.ToString(this.sessionService.GetSession().SessionId));
                jsonBody.Append("}");

                if (formData == null)
                {
                    formData = new Dictionary<string, string>();
                }

                formData.Add("json", jsonBody.ToString());
            }

            HttpResponseMessage responseMessage = await this.PostAsync(url, formData, signUrl);
            TResult result = await responseMessage.Content.ReadAsJsonObject<TResult>();

            if (result.ReloadXsrf.HasValue && result.ReloadXsrf.Value)
            {
                this.logger.Debug("PostAsync :: Reload Xsrf requested. Reloading.");
                await this.PostAsync(RefreshXtPath);

                responseMessage = await this.PostAsync(url, formData, signUrl);
                result = await responseMessage.Content.ReadAsJsonObject<TResult>();
            }

            return result;
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