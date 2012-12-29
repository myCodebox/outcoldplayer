// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Security.Credentials;

    public class UserDataStorage : IUserDataStorage
    {
        private const string GoogleAccountsResource = "OutcoldSolutions.GoogleMusic";

        private readonly ILogger logger;

        private UserSession userSession;

        public UserDataStorage(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("UserDataStorage");
        }

        public void SaveUserInfo(UserInfo userInfo)
        {
            this.logger.Debug("SaveUserInfo");

            var passwordCredential = new PasswordCredential(GoogleAccountsResource, userInfo.Email, userInfo.Password);
            PasswordVault vault = new PasswordVault();

            // Remove old
            try
            {
                var all = vault.FindAllByResource(GoogleAccountsResource);
                foreach (var credential in all)
                {
                    this.logger.Debug("Remove old password credentials.");
                    vault.Remove(credential);
                }
            }
            catch (Exception exception)
            {
                this.logger.LogDebugException(exception);
            }
            
            this.logger.Debug("Add new passwrod credentials.");
            vault.Add(passwordCredential);
        }

        public UserInfo GetUserInfo(bool retrievePassword)
        {
            this.logger.Debug("GetUserInfo");

            PasswordVault vault = new PasswordVault();

            try
            {
                var list = vault.FindAllByResource(GoogleAccountsResource);
                if (list.Count > 0)
                {
                    this.logger.Debug("Found password credentials. Count: {0}", list.Count);
                    if (retrievePassword)
                    {
                        list[0].RetrievePassword();
                    }
                    return new UserInfo(list[0].UserName, list[0].Password);
                }
            }
            catch (Exception exception)
            {
                this.logger.LogDebugException(exception);
            }

            this.logger.Debug("Password credentials could not find.");
            return null;
        }

        public void ClearUserInfo()
        {
            this.logger.Debug("ClearUserInfo");

            PasswordVault vault = new PasswordVault();

            try
            {
                var all = vault.FindAllByResource(GoogleAccountsResource);
                foreach (var credential in all)
                {
                    this.logger.Debug("Remove old password credentials.");
                    vault.Remove(credential);
                }
            }
            catch (Exception exception)
            {
                this.logger.LogDebugException(exception);
            }
        }

        public void SetUserSession(UserSession session)
        {
            this.logger.Debug("SetUserSession");
            this.userSession = session;
        }

        public UserSession GetUserSession()
        {
            this.logger.Debug("GetUserSession. User session is not null: {0}.", this.userSession != null);
            return this.userSession;
        }
    }
}