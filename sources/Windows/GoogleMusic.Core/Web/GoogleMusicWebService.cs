// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;

    public class GoogleMusicWebService : IGoogleMusicWebService
    {
        private const string OriginUrl = "https://play.google.com";
        private const string PlayMusicUrl = "https://play.google.com/music/listen?u=0&hl=en";

        private readonly IUserDataStorage userDataStorage;
        private readonly ILogger logger;

        private HttpClient httpClient;
        private HttpClientHandler httpClientHandler;

        public GoogleMusicWebService(
            ILogManager logManager,
            IUserDataStorage userDataStorage)
        {
            this.userDataStorage = userDataStorage;
            this.logger = logManager.CreateLogger("GoogleAccountWebService");
        }

        public async Task<bool> InitializeAsync(string auth)
        {
            if (auth == null)
            {
                throw new ArgumentNullException("auth");
            }

            this.httpClientHandler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };

            this.httpClient = new HttpClient(this.httpClientHandler)
                                  {
                                      BaseAddress = new Uri(OriginUrl)
                                  };

            var url = new StringBuilder("https://www.google.com/accounts/TokenAuth");
            url.Append("?");
            url.AppendFormat("auth={0}", WebUtility.UrlEncode(auth));
            url.AppendFormat("&service=sj");
            url.AppendFormat("&continue={0}", WebUtility.UrlEncode(PlayMusicUrl));
            var requestUri = url.ToString();

            var getCookieResponse = await this.httpClient.GetAsync(requestUri);
            await this.LogResponseAsync(requestUri, getCookieResponse);

            return this.httpClientHandler.CookieContainer.Count > 0;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            this.logger.Debug("GetAsync: {0}.", url);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            var responseMessage = await this.httpClient.SendAsync(httpRequestMessage);

            await this.LogResponseAsync(url, responseMessage);

            this.VerifyAuthorization(responseMessage);

            return responseMessage;
        }

        public async Task<HttpResponseMessage> PostAsync(
            string url, 
            IDictionary<HttpRequestHeader, string> headers = null,
            IDictionary<string, string> formData = null)
        {
            var cookieCollection = this.httpClientHandler.CookieContainer.GetCookies(new Uri(PlayMusicUrl));

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("-----------------------");

                this.logger.Debug("PostAsync: {0}.", url);

                if (headers != null)
                {
                    this.logger.Debug("    HEADERS: ");

                    foreach (var header in headers)
                    {
                        this.logger.Debug("        {0}={1}", header.Key, header.Value);
                    }
                }

                if (formData != null)
                {
                    this.logger.Debug("    FORMDATA: ");

                    foreach (var argument in formData)
                    {
                        this.logger.Debug("        {0}={1}", argument.Key, argument.Value);
                    }
                }

                this.logger.Debug("    COOKIES({0}):", cookieCollection.Count);

                foreach (Cookie cookieLog in cookieCollection)
                {
                    this.logger.Debug("        {0}", cookieLog.ToString());
                }

                this.logger.Debug("-----------------------");
            }

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

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    requestMessage.Headers.Add(header.Key.ToString(), header.Value);
                }
            }

            if (formData != null)
            {
                requestMessage.Content = new FormUrlEncodedContent(formData);
            }

            var responseMessage = await this.httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);

            await this.LogResponseAsync(url, responseMessage);

            this.VerifyAuthorization(responseMessage);

            return responseMessage;
        }

        private async Task LogResponseAsync(string url, HttpResponseMessage httpResponseMessage)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("-----------------------");
                this.logger.Debug("Request '{0}' completed, Status code: {1}.", url, httpResponseMessage.StatusCode);

                this.logger.Debug("    RESPONSE HEADERS: ");

                foreach (var httpResponseHeader in httpResponseMessage.Headers)
                {
                    this.logger.Debug("        {0}={1}", httpResponseHeader.Key, string.Join("&&&", httpResponseHeader.Value));
                }

                if (httpResponseMessage.Content != null)
                {
                    this.logger.Debug("    RESPONSE CONTENT HEADERS: ");

                    foreach (var header in httpResponseMessage.Content.Headers)
                    {
                        this.logger.Debug("        {0}={1}", header.Key, string.Join("&&&", header.Value));
                    }

                    if (httpResponseMessage.Content.Headers.ContentType.IsPlainText()
                        || httpResponseMessage.Content.Headers.ContentType.IsHtmlText())
                    {
                        using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync())
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                char[] buffer = new char[4096];
                                var read = await reader.ReadAsync(buffer, 0, buffer.Length);

                                if (read > 0)
                                {
                                    var bodyData = new StringBuilder();
                                    bodyData.Append(buffer, 0, read);

                                    this.logger.Debug("    RESPONSE CONTENT:{0}{1}", Environment.NewLine, bodyData);
                                    this.logger.Debug("    RESPONSE ENDCONTENT.");
                                }
                            }
                        }
                    }
                }
                else
                {
                    this.logger.Debug("CONTENT is null.");
                }

                this.logger.Debug("-----------------------");
            }
        }

        private void VerifyAuthorization(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    this.userDataStorage.ClearSession();
                    responseMessage.EnsureSuccessStatusCode();
                }
            }
        }
    }
}