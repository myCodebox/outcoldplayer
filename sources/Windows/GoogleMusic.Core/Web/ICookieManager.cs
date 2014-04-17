// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Net;

    public interface ICookieManager
    {
        CookieContainer GetCookies();

        void SaveCookies(Uri uri, CookieCollection cookies);
    }
}