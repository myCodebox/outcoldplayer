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
        private readonly static Uri BaseUri = new Uri("https://play.google.com/music/");
        private const string SecureHeaderValue = " Secure";

        private readonly CookieContainer cookieContainer = new CookieContainer();
        private readonly string localPath = "/";

        public CookieContainerWrapper()
        {
            if (!string.Equals(BaseUri.LocalPath, this.localPath))
            {
                this.localPath = BaseUri.LocalPath.TrimEnd('/');
            }
        }

        public string GetCookieHeader()
        {
            return this.GetCookieHeader(BaseUri);
        }

        public string GetCookieHeader(Uri uri)
        {
            return this.cookieContainer.GetCookieHeader(uri);
        }

        public IEnumerable<Cookie> GetCookies()
        {
            return this.GetCookies(BaseUri);
        }

        public IEnumerable<Cookie> GetCookies(Uri uri)
        {
            return this.cookieContainer.GetCookies(BaseUri).Cast<Cookie>();
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
                if (string.Equals(cookie.Domain, BaseUri.Host))
                {
                    cookie.Path = this.localPath;
                }

                this.cookieContainer.Add(BaseUri, cookie);
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

            this.VerifyCookieHeaderProperty(ref cookieHeader, "Domain", BaseUri.Host);
            this.VerifyCookieHeaderProperty(ref cookieHeader, "Path", this.localPath);

            this.cookieContainer.SetCookies(BaseUri, cookieHeader);
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