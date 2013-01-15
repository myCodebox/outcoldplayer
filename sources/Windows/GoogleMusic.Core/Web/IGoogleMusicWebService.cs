// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IGoogleMusicWebService
    {
        string GetServiceUrl();

        void Initialize(CookieCollection cookieCollection);

        CookieCollection GetCurrentCookies();

        Task<HttpResponseMessage> GetAsync(
            string url, 
            bool authenticated = true);

        Task<HttpResponseMessage> PostAsync(
            string url,
            IDictionary<string, string> formData = null, 
            bool authenticated = true);
    }
}