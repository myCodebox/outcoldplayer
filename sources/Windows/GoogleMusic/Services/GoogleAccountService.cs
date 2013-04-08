// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Security.Credentials;

    public class GoogleAccountService : IGoogleAccountService
    {
        private const string GoogleAccountsResource = "OutcoldSolutions.GoogleMusic";

        private readonly ILogger logger;

        public GoogleAccountService(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("GoogleAccountService");
        }

        public void SetUserInfo(UserInfo info)
        {
            this.logger.Debug("SetUserInfo. Is null {0}.", info == null);
            
            if (info != null && info.RememberAccount)
            {
                this.logger.Debug("SetUserInfo: RememberAccount");

                var passwordCredential = new PasswordCredential(GoogleAccountsResource, info.Email, info.Password);
                PasswordVault vault = new PasswordVault();

                // Remove old
                try
                {
                    var all = vault.FindAllByResource(GoogleAccountsResource);
                    foreach (var credential in all)
                    {
                        this.logger.Debug("SetUserInfo: Remove old password credentials.");
                        vault.Remove(credential);
                    }
                }
                catch (Exception)
                {
                    this.logger.Debug("Exception while tried to remove credentials");
                }

                this.logger.Debug("SetUserInfo: Adding new passwrod credentials.");
                vault.Add(passwordCredential);
            }
        }

        public UserInfo GetUserInfo(bool retrievePassword = false)
        {
            this.logger.Debug("GetUserInfo");

            PasswordVault vault = new PasswordVault();

            try
            {
                var list = vault.FindAllByResource(GoogleAccountsResource);
                if (list.Count > 0)
                {
                    this.logger.Debug("GetUserInfo: Found password credentials. Count: {0}", list.Count);
                    if (retrievePassword)
                    {
                        this.logger.Debug("GetUserInfo: Retrieving password.");
                        list[0].RetrievePassword();
                    }

                    return new UserInfo(list[0].UserName, list[0].Password) { RememberAccount = true };
                }
            }
            catch (Exception)
            {
                this.logger.Debug("Exception while tried to retrieve user info.");
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
            catch (Exception)
            {
                this.logger.Debug("Exception while tried to remove user info.");
            }
        }
    }
}