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
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Windows.Security.Credentials;
    using Windows.Security.Cryptography;
    using Windows.Security.Cryptography.Core;
    using Windows.Storage.Streams;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    public class LastfmWebService : WebServiceBase, ILastfmWebService
    {
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

        public LastfmWebService(ILogManager logManager)
        {
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
                PasswordVault vault = new PasswordVault();

                this.ClearAllPasswordCredentials(vault);

                this.logger.Debug("SaveCurrentSessionAsync: Adding new password credentials.");

                var session = new PasswordCredential(
                    LastFmSessionResource,
                    this.currentSession.Name,
                    string.Format("{0}:::{1}", this.sessionToken, this.currentSession.Key));

                vault.Add(session);

                this.dirty = false;
            }
        }

        public bool RestoreSession()
        {
            PasswordVault vault = new PasswordVault();

            // Remove old
            try
            {
                var all = vault.FindAllByResource(LastFmSessionResource);
                PasswordCredential session = all.FirstOrDefault();
                if (session != null)
                {
                    session.RetrievePassword();

                    var keys = session.Password.Split(new[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    if (keys.Length == 2)
                    {
                        this.sessionToken = keys[0];
                        this.currentSession = new Session() { Key = keys[1], Name = session.UserName };
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
            PasswordVault vault = new PasswordVault();
            this.ClearAllPasswordCredentials(vault);
            this.currentSession = null;
            this.sessionToken = null;
            this.dirty = false;
        }

        public Session GetSession()
        {
            return this.currentSession;
        }

        private void ClearAllPasswordCredentials(PasswordVault vault)
        {
            try
            {
                var all = vault.FindAllByResource(LastFmSessionResource);
                foreach (var credential in all)
                {
                    this.logger.Debug("SaveCurrentSessionAsync: Remove old sessions.");
                    vault.Remove(credential);
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
                    this.logger.Debug("ClearAllPasswordCredentials: Not found.");
                }
            }
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

            HashAlgorithmProvider hashProvider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer hash = hashProvider.HashData(CryptographicBuffer.ConvertStringToBinary(signature.ToString(), BinaryStringEncoding.Utf8));
            return BitConverter.ToString(hash.ToArray()).Replace("-", string.Empty);
        }
    }
}