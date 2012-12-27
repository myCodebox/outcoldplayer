// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using OutcoldSolutions.GoogleMusic.Models;

    public interface IUserDataStorage
    {
        void SaveUserInfo(UserInfo userInfo);

        UserInfo GetUserInfo();

        void ClearUserInfo();

        void SetUserSession(UserSession session);

        UserSession GetUserSession();
    }
}