// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class GoogleWebService : IGoogleWebService
    {
        private readonly IDependencyResolverContainer container;
        private readonly IUserDataStorage userDataStorage;
        private readonly ILogger logger;

        public GoogleWebService(
            IDependencyResolverContainer container, 
            ILogManager logManager,
            IUserDataStorage userDataStorage)
        {
            this.container = container;
            this.userDataStorage = userDataStorage;
            this.logger = logManager.CreateLogger("ClientLoginService");
        }

        public Task<GoogleWebResponse> GetAsync(
            string url, 
            IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, 
            IEnumerable<KeyValuePair<string, string>> arguments = null)
        {
            var taskCompletionSource = new TaskCompletionSource<GoogleWebResponse>();

            Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        var httpWebRequest = this.SetupRequest(url, "GET", headers);

                        try
                        {
                            this.HandleGetResponse(httpWebRequest, taskCompletionSource);
                        }
                        catch (Exception exception)
                        {
                            logger.LogErrorException(exception);

                            this.HandleRequestException(exception, taskCompletionSource);
                        }
                    }
                    catch (Exception exception)
                    {
                        logger.LogErrorException(exception);
                        taskCompletionSource.SetException(exception);
                    }
                });

            return taskCompletionSource.Task;
        }

        public Task<GoogleWebResponse> PostAsync(
            string url, 
            IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, 
            IEnumerable<KeyValuePair<string, string>> arguments = null)
        {
            var taskCompletionSource = new TaskCompletionSource<GoogleWebResponse>();

            Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        var httpWebRequest = this.SetupRequest(url, "POST", headers);

                        httpWebRequest.BeginGetRequestStream(
                            (asyncResult) =>
                            {
                                try
                                {
                                    using (var requestStream = httpWebRequest.EndGetRequestStream(asyncResult))
                                    {
                                        if (arguments != null)
                                        {
                                            using (var writer = new RequestBodyWriter(requestStream))
                                            {
                                                foreach (var argument in arguments)
                                                {
                                                    writer.Add(argument.Key, argument.Value);
                                                }
                                            }
                                        }
                                    }

                                    this.HandleGetResponse(httpWebRequest, taskCompletionSource);
                                }
                                catch (Exception exception)
                                {
                                    logger.LogErrorException(exception);

                                    this.HandleRequestException(exception, taskCompletionSource);
                                }
                            },
                            null);
                    }
                    catch (Exception exception)
                    {
                        logger.LogErrorException(exception);
                        taskCompletionSource.SetException(exception);
                    }
                });

            return taskCompletionSource.Task;
        }

        private HttpWebRequest SetupRequest(string url, string method, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null)
        {
            var userSession = this.userDataStorage.GetUserSession();
            if (userSession != null && method == "POST")
            {
                if (userSession.Cookies != null && userSession.Cookies.Count > 0)
                {
                    if (url.IndexOf("?", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        url += "?";
                    }
                    else
                    {
                        url += "&";
                    }

                    url += string.Format("u=0&{0}", userSession.Cookies["xt"]);
                }
            }
            
            var httpWebRequest = WebRequest.CreateHttp(url);

            if (method == "POST")
            {
                httpWebRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            }

            httpWebRequest.Method = method;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    httpWebRequest.Headers[header.Key] = header.Value;
                }
            }

            if (userSession != null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
                if (userSession.Cookies != null)
                {
                    foreach (Cookie cookie in userSession.Cookies)
                    {
                        httpWebRequest.CookieContainer.Add(new Uri(url), cookie);
                    }
                }

                httpWebRequest.Headers[HttpRequestHeader.Authorization] =
                    string.Format(CultureInfo.InvariantCulture, "GoogleLogin auth={0}", userSession.Auth);
            }

            return httpWebRequest;
        }

        private void HandleGetResponse(HttpWebRequest httpWebRequest, TaskCompletionSource<GoogleWebResponse> taskCompletionSource)
        {
            httpWebRequest.BeginGetResponse(
                (responceAsyncResult) =>
                    {
                        try
                        {
                            var webResponse = httpWebRequest.EndGetResponse(responceAsyncResult);
                            taskCompletionSource.SetResult(this.ParseResponse(webResponse));
                        }
                        catch (WebException exception)
                        {
                            if (((HttpWebResponse)exception.Response).StatusCode == HttpStatusCode.Unauthorized)
                            {
                                this.userDataStorage.ClearSession();
                            }

                            this.logger.LogDebugException(exception);
                            taskCompletionSource.SetResult(this.ParseResponse(exception.Response));
                        }
                        catch (Exception exception)
                        {
                            this.logger.LogErrorException(exception);
                            taskCompletionSource.SetException(exception);
                        }
                    },
                null);
        }

        private void HandleRequestException(Exception exception, TaskCompletionSource<GoogleWebResponse> taskCompletionSource)
        {
            var webException = exception as WebException;
            if (webException != null)
            {
                var webResponse = webException.Response;
                taskCompletionSource.SetResult(this.ParseResponse(webResponse));
            }
            else
            {
                taskCompletionSource.SetException(exception);
            }
        }

        private GoogleWebResponse ParseResponse(WebResponse webResponse)
        {
            var httpWebResponse = (HttpWebResponse)webResponse;
            return new GoogleWebResponse(this.container.Resolve<ILogManager>(), httpWebResponse);
        }
    }
}