// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Net;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IUserDataStorage
    {
        void SaveUserInfo(UserInfo userInfo);

        UserInfo GetUserInfo();

        void SaveCookies(Uri url, CookieCollection cookieCollection);

        CookieContainer GetCookieContainer();
    }
}