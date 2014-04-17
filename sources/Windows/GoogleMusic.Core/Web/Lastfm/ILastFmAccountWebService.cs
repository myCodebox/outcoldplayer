// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Lastfm
{
    using System.Threading.Tasks;

    public interface ILastfmAccountWebService
    {
        Task<TokenResp> GetTokenAsync();

        Task<GetSessionResp> GetSessionAsync(string token);

        string GetAuthUrl(string token);
    }
}