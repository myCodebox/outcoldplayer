// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public interface IClientLoginService
    {
        Task<GoogleWebResponse> LoginAsync(string email, string password);

        Task<GoogleWebResponse> GetCookieAsync(string auth);
    }
}