﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Net;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IGoogleMusicSessionService
    {
        event EventHandler SessionCleared;

        UserSession GetSession();

        void SaveCurrentSession(CookieCollection cookieCollection);

        void LoadSession();

        CookieCollection GetSavedCookies();

        void ClearSession();
    }
}