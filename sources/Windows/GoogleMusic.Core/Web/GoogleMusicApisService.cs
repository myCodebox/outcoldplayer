// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.Web;

    public class GoogleMusicApisService : WebServiceBase, IGoogleMusicApisService
    {
        private const string OriginUrl = "https://www.googleapis.com/sj/v1.3/";

        private readonly ILogger logger;
        private readonly HttpClient httpClient;

        private readonly IGoogleMusicSessionService sessionService;

        public GoogleMusicApisService(
            ILogManager logManager,
            IGoogleMusicSessionService sessionService)
        {
            var httpClientHandler = new HttpClientHandler
            {
                UseCookies = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip
            };

            this.httpClient = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri(OriginUrl),
                Timeout = TimeSpan.FromSeconds(90)
            };

            this.sessionService = sessionService;

            this.logger = logManager.CreateLogger("GoogleMusicApisService");
        }

        protected override ILogger Logger
        {
            get
            {
                return this.logger;
            }
        }

        protected override HttpClient HttpClient
        {
            get
            {
                return this.httpClient;
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            if (this.Logger.IsDebugEnabled)
            {
                this.Logger.LogRequest(HttpMethod.Get, url, null);
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, this.UrlDefaultParameters(url));
            requestMessage.Headers.Add("Authorization", this.GetAuthorizationHeaderValue());
            requestMessage.Headers.Add("Accept-Encoding", "gzip");

            var responseMessage = await this.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

            return responseMessage;
        }

        public async Task<HttpResponseMessage> PostAsync(string url, dynamic json = null, bool signUrl = false)
        {
            if (this.Logger.IsDebugEnabled)
            {
                this.Logger.LogRequest(HttpMethod.Post, url, null, (object)json);
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, this.UrlDefaultParameters(url));
            requestMessage.Headers.Add("Authorization", this.GetAuthorizationHeaderValue());
            if (json != null)
            {
                var content = json is string ? (string)json : (string)JsonConvert.SerializeObject(json);
                requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }

            var responseMessage = await this.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);

            return responseMessage;
        }

        public async Task<TResult> GetAsync<TResult>(string url)
        {
            HttpResponseMessage responseMessage = null;
            HttpRequestException exception = null;
            try
            {
                responseMessage = await this.GetAsync(url);

                // This means that google asked us to relogin. Let's try again this request.
                if (responseMessage.StatusCode == HttpStatusCode.Found
                    || responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    responseMessage = await this.GetAsync(url);
                }

                responseMessage.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                exception = e;
            }

            if (exception != null)
            {
                StringBuilder errorMessage = new StringBuilder();
                HttpStatusCode statusCode = 0;
                if (responseMessage != null)
                {
                    statusCode = responseMessage.StatusCode;

                    var response = await responseMessage.Content.ReadAsJsonObject<GoogleMusicErrorResponse>();
                    if (response.Error != null)
                    {
                        errorMessage.AppendFormat("Code: {0}, Message: {1}", response.Error.Code, response.Error.Message);
                    }
                    else
                    {
                        errorMessage.AppendFormat(
                            CultureInfo.CurrentCulture,
                            "Exception while we tried to get resposne for url (GET) '{0}'. {1}",
                            url,
                            exception.Message);
                    }
                }

                throw new GoogleApiWebRequestException(errorMessage.ToString(), exception, statusCode);
            }

            return await responseMessage.Content.ReadAsJsonObject<TResult>();
        }

        public async Task<TResult> PostAsync<TResult>(string url, dynamic json = null, bool signUrl = false)
        {
            HttpResponseMessage responseMessage = null;
            HttpRequestException exception = null;

            try
            {
                responseMessage = await this.PostAsync(url, json, signUrl);

                // This means that google asked us to relogin. Let's try again this request.
                if (responseMessage.StatusCode == HttpStatusCode.Found
                    || responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    responseMessage = await this.PostAsync(url, json, signUrl);
                }

                responseMessage.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                exception = e;
            }

            if (exception != null)
            {
                StringBuilder errorMessage = new StringBuilder();
                HttpStatusCode statusCode = 0;
                if (responseMessage != null)
                {
                    statusCode = responseMessage.StatusCode;

                    var response = await responseMessage.Content.ReadAsJsonObject<GoogleMusicErrorResponse>();
                    if (response.Error != null)
                    {
                        errorMessage.AppendFormat("Code: {0}, Message: {1}", response.Error.Code, response.Error.Message);
                    }
                    else
                    {
                        errorMessage.AppendFormat(
                            CultureInfo.CurrentCulture,
                            "Exception while we tried to get resposne for url (POST) '{0}'. {1}",
                            url,
                            exception.Message);

                        if (responseMessage.StatusCode == HttpStatusCode.Found)
                        {
                            errorMessage.AppendFormat(
                                CultureInfo.CurrentCulture, ". 302: Moved to '{0}'", responseMessage.Headers.Location.LocalPath);
                        }
                    }
                }
                
                throw new GoogleApiWebRequestException(errorMessage.ToString(), exception, statusCode);
            }

            return await responseMessage.Content.ReadAsJsonObject<TResult>();
        }

        public async Task<IList<TData>> DownloadList<TData>(
            string url, 
            DateTime? lastUpdate = null,
            IProgress<int> progress = null, 
            Func<IList<TData>, Task> chunkHandler = null)
        {
            List<TData> result = new List<TData>();

            Task commitTask = Task.FromResult<object>(null);
            string startToken = "0";

            if (lastUpdate.HasValue)
            {
                if (url.IndexOf('?') < 0)
                {
                    url += "?";
                }
                else
                {
                    url += "&";
                }

                url += "updated-min=" + ((ulong)lastUpdate.Value.ToUnixFileTime() * 1000L).ToString("G", CultureInfo.InvariantCulture);
            }

            do
            {
                Task<GoogleListResponse<TData>> loadSongsTask = this.PostAsync<GoogleListResponse<TData>>(
                    url,
                    string.Format("{{\"max-results\":250,\"start-token\":\"{0}\"}}", startToken));

                await Task.WhenAll(commitTask, loadSongsTask);

                GoogleListResponse<TData> playlist = await loadSongsTask;

                startToken = playlist.NextPageToken;

                if (playlist.Data != null && playlist.Data.Items != null)
                {
                    commitTask = chunkHandler == null
                        ? Task.FromResult<object>(null)
                        : chunkHandler(playlist.Data.Items);

                    result.AddRange(playlist.Data.Items);
                }

                await progress.SafeReportAsync(result.Count);
            }
            while (!string.IsNullOrEmpty(startToken));

            await commitTask;

            return result;
        }

        private string UrlDefaultParameters(string url)
        {
            if (url.IndexOf('?') < 0)
            {
                url += "?";
            }
            else
            {
                url += "&";
            }

            url += "alt=json&hl=" + CultureInfo.CurrentCulture.Name.Replace("-", "_");

            return url;
        }

        private string GetAuthorizationHeaderValue()
        {
            var auth = this.sessionService.GetAuth();
            
            string value = string.Empty;

            if (auth != null)
            {
                value = string.Format("GoogleLogin auth={0}", auth);
            }
            else
            {
                this.Logger.Debug("Token is null.");
            }

            return value;
        }
    }
}
