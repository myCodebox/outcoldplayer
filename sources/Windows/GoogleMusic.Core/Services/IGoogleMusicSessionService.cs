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

    public interface IGoogleMusicSessionService
    {
        event EventHandler SessionCleared;

        UserSession GetSession();

        void LoadSession();

        Task SaveCurrentSessionAsync(IEnumerable<Cookie> cookieCollection);

        Task<IEnumerable<Cookie>> GetSavedCookiesAsync();

        Task ClearSession();
    }
}