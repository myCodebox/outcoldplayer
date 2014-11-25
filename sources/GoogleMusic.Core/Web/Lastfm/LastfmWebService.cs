// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Lastfm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;

    public class LastfmWebService : WebServiceBase, ILastfmWebService
    {
        private readonly ISecureStorageService secureStorageService;
        private readonly IDataProtectService dataProtectService;
        public const string ApiKey = "92fa0e285e2204582fbb359321567658";
        private const string LastFmSessionResource = "OutcoldSolutions.LastFm";

        private readonly ILogger logger;

        private readonly HttpClient httpClient = new HttpClient()
                                                     {
                                                         BaseAddress = new Uri("http://ws.audioscrobbler.com/2.0/")
                                                     };

        private string sessionToken;
        private Session currentSession;
        private bool dirty = false;

        public LastfmWebService(ILogManager logManager, ISecureStorageService secureStorageService, IDataProtectService dataProtectService)
        {
            this.secureStorageService = secureStorageService;
            this.dataProtectService = dataProtectService;
            this.logger = logManager.CreateLogger("LastfmWebService");
        }

        protected override ILogger Logger
        {
            get { return this.logger; }
        }

        protected override HttpClient HttpClient
        {
            get { return this.httpClient; }
        }

        public async Task<HttpResponseMessage> CallAsync(string methodName, IDictionary<string, string> parameters = null, CancellationToken? cancellationToken = null)
        {
            var urlBuilder = new StringBuilder("?");

            var requestParameters = parameters == null
                                        ? new SortedDictionary<string, string>()
                                        : new SortedDictionary<string, string>(parameters);

            requestParameters.Add("method", methodName);
            requestParameters.Add("api_key", ApiKey);

            if (this.currentSession != null && !string.IsNullOrEmpty(this.currentSession.Key))
            {
                requestParameters.Add("sk", this.currentSession.Key);
            }

            if (this.IsSignatureRequired())
            {
                urlBuilder.AppendFormat("&api_sig={0}", WebUtility.UrlEncode(this.GetSignature(requestParameters)));
            }

            urlBuilder.Append("&format=json");

            foreach (var parameter in requestParameters)
            {
                urlBuilder.AppendFormat("&{0}={1}", WebUtility.UrlEncode(parameter.Key), WebUtility.UrlEncode(parameter.Value));
            }
            
            string url = urlBuilder.ToString();

            return await this.SendAsync(new HttpRequestMessage(HttpMethod.Post, url), HttpCompletionOption.ResponseContentRead, cancellationToken);
        }

        public void SetToken(string token)
        {
            this.dirty = true;
            this.sessionToken = token;
        }

        public void SetSession(Session session)
        {
            this.dirty = true;
            this.currentSession = session;

            this.SaveCurrentSession();
        }

        public void SaveCurrentSession()
        {
            if (this.currentSession != null && this.dirty)
            {
                this.ClearAllPasswordCredentials();
                this.logger.Debug("SaveCurrentSessionAsync: Adding new passwrod credentials.");
                this.secureStorageService.Save(LastFmSessionResource, this.sessionToken, string.Format("{0}:::{1}", this.sessionToken, this.currentSession.Key));

                this.dirty = false;
            }
        }

        public bool RestoreSession()
        {
            // Remove old
            try
            {
                string username, password;
                if (this.secureStorageService.Get(LastFmSessionResource, out username, out password))
                {
                    var keys = password.Split(new[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    if (keys.Length == 2)
                    {
                        this.sessionToken = keys[0];
                        this.currentSession = new Session() { Key = keys[1], Name = username };
                        this.dirty = false;
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                if (((uint)exception.HResult) != 0x80070490)
                {
                    this.logger.Error(exception, "Exception while tried to ClearAllPasswordCredentials.");
                }
                else
                {
                    this.logger.Debug("RestoreSession: Not found.");
                }
            }

            return false;
        }

        public void ForgetAccount()
        {
            this.ClearAllPasswordCredentials();
            this.currentSession = null;
            this.sessionToken = null;
            this.dirty = false;
        }

        public Session GetSession()
        {
            return this.currentSession;
        }

        private void ClearAllPasswordCredentials()
                {
            this.secureStorageService.Delete(LastFmSessionResource);
        }

        private bool IsSignatureRequired()
        {
            return this.sessionToken != null;
        }

        private string GetSignature(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            StringBuilder signature = new StringBuilder();
            foreach (var parameter in parameters)
            {
                signature.Append(parameter.Key);
                signature.Append(parameter.Value);
            }

            signature.Append("e2887c9308f77280e24f8a75fdeb375f");

            return BitConverter.ToString(this.dataProtectService.GetMd5Hash(signature.ToString())).Replace("-", string.Empty);
        }
    }
}