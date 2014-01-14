// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IGoogleMusicWebService
    {
        string GetServiceUrl();

        void Initialize(IEnumerable<Cookie> cookieCollection);

        IEnumerable<Cookie> GetCurrentCookies();

        Task RefreshXtAsync();

        Task SaveCurrentSessionAsync();

        Task<HttpResponseMessage> GetAsync(
            string url, 
            bool signUrl = true);

        Task<HttpResponseMessage> PostAsync(
            string url,
            IDictionary<string, string> formData = null, 
            bool signUrl = true);

        Task<TResult> GetAsync<TResult>(
            string url,
            bool signUrl = true) where TResult : CommonResponse;

        Task<TResult> PostAsync<TResult>(
            string url,
            IDictionary<string, string> formData = null,
            IDictionary<string, string> jsonProperties = null, 
            bool forceJsonBody = true,
            bool signUrl = true) where TResult : CommonResponse;
    }
}