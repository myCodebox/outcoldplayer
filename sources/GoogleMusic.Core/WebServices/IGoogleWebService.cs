// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public interface IGoogleWebService
    {
        Task<GoogleWebResponse> GetAsync(
            string url, 
            IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null);

        Task<GoogleWebResponse> PostAsync(
            string url, 
            IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, 
            IEnumerable<KeyValuePair<string, string>> arguments = null);
    }
}