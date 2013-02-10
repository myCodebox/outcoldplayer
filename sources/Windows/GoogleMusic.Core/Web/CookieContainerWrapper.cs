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

    public class CookieContainerWrapper
    {
        private const string SecureHeaderValue = " Secure";

        private readonly CookieContainer cookieContainer = new CookieContainer();
        private readonly Uri baseUri;
        private readonly string localPath = "/";

        public CookieContainerWrapper(Uri baseUri)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }

            this.baseUri = baseUri;

            if (!string.Equals(this.baseUri.LocalPath, this.localPath))
            {
                this.localPath = this.baseUri.LocalPath.TrimEnd('/');
            }
        }

        public string GetCookieHeader()
        {
            return this.cookieContainer.GetCookieHeader(this.baseUri);
        }

        public IEnumerable<Cookie> GetCookies()
        {
            return this.cookieContainer.GetCookies(this.baseUri).Cast<Cookie>();
        }

        public Cookie FindCookie(string name)
        {
            return this.GetCookies()
                .FirstOrDefault(cookie => string.Equals(cookie.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public void AddCookies(IEnumerable<Cookie> cookies)
        {
            if (cookies == null)
            {
                throw new ArgumentNullException("cookies");
            }

            foreach (var cookie in cookies)
            {
                if (string.Equals(cookie.Domain, this.baseUri.Host))
                {
                    cookie.Path = this.localPath;
                }

                this.cookieContainer.Add(this.baseUri, cookie);
            }
        }

        public void SetCookies(IEnumerable<string> cookieHeaders)
        {
            if (cookieHeaders == null)
            {
                throw new ArgumentNullException("cookieHeaders");
            }

            foreach (var responseCookie in cookieHeaders)
            {
                this.SetCookies(responseCookie);
            }
        }

        private void SetCookies(string cookieHeader)
        {
            if (cookieHeader == null)
            {
                throw new ArgumentNullException("cookieHeader");
            }

            this.VerifyCookieHeaderProperty(ref cookieHeader, "Domain", this.baseUri.Host);
            this.VerifyCookieHeaderProperty(ref cookieHeader, "Path", this.localPath);

            this.cookieContainer.SetCookies(this.baseUri, cookieHeader);
        }

        private void VerifyCookieHeaderProperty(ref string cookieHeader, string property, string defaultValue)
        {
            if (cookieHeader.IndexOf(string.Format(CultureInfo.InvariantCulture, ";{0}=", property), StringComparison.OrdinalIgnoreCase) <= 0)
            {
                int insertIndex = cookieHeader.Length;
                if (cookieHeader.IndexOf(SecureHeaderValue, StringComparison.OrdinalIgnoreCase) == (cookieHeader.Length - SecureHeaderValue.Length))
                {
                    insertIndex = cookieHeader.LastIndexOf(";", StringComparison.OrdinalIgnoreCase);
                }

                cookieHeader = cookieHeader.Insert(insertIndex, string.Format(CultureInfo.InvariantCulture, ";{0}={1}", property, defaultValue));
            }
        }
    }
}