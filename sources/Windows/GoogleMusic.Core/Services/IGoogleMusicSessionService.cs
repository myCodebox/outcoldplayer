// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IGoogleMusicSessionService
    {
        event EventHandler SessionCleared;

        UserSession GetSession();

        void LoadSession();

        Task SaveCurrentSessionAsync(CookieCollection cookieCollection);

        Task<CookieCollection> GetSavedCookiesAsync();

        void ClearSession();
    }
}