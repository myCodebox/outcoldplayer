// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System.Net;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IGoogleAccountWebService
    {
        Task<GoogleLoginResponse> AuthenticateAsync(string email, string password);

        Task<CookieCollection> GetCookiesAsync(string redirectUrl);
    }
}