// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Net;

    public class UserSession
    {
        public UserSession(string auth, CookieContainer cookieContainer)
        {
            this.Auth = auth;
            this.CookieContainer = cookieContainer;

            char[] s = new char[12];
            var random = new Random();

            for (int i = 0; i < s.Length; i++)
            {
                if (random.Next(1, 4) == 1)
                {
                    s[i] = Convert.ToChar(random.Next(0x30, 0x39)); // 0 - 9
                }
                else
                {
                    s[i] = Convert.ToChar(random.Next(0x61, 0x7A)); // a - z
                }
            }

            this.SessionId = new string(s);
        }

        public string Auth { get; private set; }

        public string SessionId { get; private set; }

        public CookieContainer CookieContainer { get; private set; }

        public CookieCollection Cookies { get; set; }
    }
}