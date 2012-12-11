// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Threading.Tasks;

    public interface IUserAuthorizationDataService
    {
        Task<UserInfo> GetUserSecurityDataAsync();
    }
}