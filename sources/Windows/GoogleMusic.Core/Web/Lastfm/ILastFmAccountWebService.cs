// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.LastFm
{
    using System.Threading.Tasks;

    public interface ILastfmAccountWebService
    {
        Task<string> GetTokenAsync();

        Task<string> GetSessionAsync(string token);
    }
}