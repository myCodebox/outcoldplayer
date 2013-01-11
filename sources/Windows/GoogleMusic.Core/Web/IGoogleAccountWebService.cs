// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IGoogleAccountWebService
    {
        Task<GoogleLoginResponse> LoginAsync(string email, string password);
    }
}