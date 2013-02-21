// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IGoogleAccountWebService
    {
        Task<GoogleAuthResponse> AuthenticateAsync(Uri serviceUri, string email, string password);
    }
}