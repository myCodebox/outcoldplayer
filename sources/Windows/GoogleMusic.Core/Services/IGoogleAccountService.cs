// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using OutcoldSolutions.GoogleMusic.Models;

    public interface IGoogleAccountService
    {
        void SetUserInfo(UserInfo info);

        UserInfo GetUserInfo();

        void ClearUserInfo();
    }
}