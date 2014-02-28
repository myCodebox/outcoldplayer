// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web;

    public interface IGoogleMusicSessionService
    {
        event EventHandler SessionCleared;

        void InitializeCookieContainer(IEnumerable<Cookie> cookieCollection, string authValue);

        CookieContainerWrapper GetCookieContainer();

        string GetAuth();

        UserSession GetSession();

        void LoadSession();

        Task SaveCurrentSessionAsync();

        Task<IEnumerable<Cookie>> GetSavedCookiesAsync();

        Task ClearSession();
    }
}