// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Lastfm
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface ILastfmWebService
    {
        Task<HttpResponseMessage> CallAsync(string methodName, IDictionary<string, string> parameters = null);

        void SetToken(string token);

        void SetSession(Session session);

        void SaveCurrentSession();

        bool RestoreSession();

        void ForgetAccount();

        Session GetSession();
    }
}