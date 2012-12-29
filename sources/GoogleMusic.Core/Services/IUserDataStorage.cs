// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IUserDataStorage
    {
        event EventHandler SessionCleared;

        void SetUserInfo(UserInfo info);

        UserInfo GetUserInfo();

        void ClearUserInfo();

        void SetUserSession(UserSession session);

        UserSession GetUserSession();

        void ClearSession();
    }
}