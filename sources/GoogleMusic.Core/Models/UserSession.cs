// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Net;

    public class UserSession
    {
        public UserSession(string auth, CookieContainer cookieContainer)
        {
            this.Auth = auth;
            this.CookieContainer = cookieContainer;
        }

        public string Auth { get; private set; }

        public CookieContainer CookieContainer { get; private set; }

        public CookieCollection Cookies { get; set; }
    }
}