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
        Task<bool> InitializeAsync(string auth);

        Task<HttpResponseMessage> GetAsync(string url);

        Task<HttpResponseMessage> PostAsync(
            string url,
            IDictionary<HttpRequestHeader, string> headers = null,
            IDictionary<string, string> formData = null);
    }
}