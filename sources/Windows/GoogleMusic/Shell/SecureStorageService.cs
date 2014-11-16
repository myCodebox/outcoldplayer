// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;
    using Windows.Security.Credentials;
    using OutcoldSolutions.GoogleMusic.Diagnostics;

    public class SecureStorageService : ISecureStorageService
    {
        private readonly ILogger logger;

        public SecureStorageService(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("SecureStorageService");
        }

        public void Save(string resource, string username, string password)
        {
            var passwordCredential = new PasswordCredential(resource, username, password);
            PasswordVault vault = new PasswordVault();

            // Remove old
            try
            {
                var all = vault.FindAllByResource(resource);
                foreach (var credential in all)
                {
                    this.logger.Debug("Save: Remove old password credentials.");
                    vault.Remove(credential);
                }
            }
            catch (Exception exception)
            {
                if (((uint)exception.HResult) != 0x80070490)
                {
                    this.logger.Warning(exception, "Exception while tried to SetUserInfo.");
                }
                else
                {
                    this.logger.Debug("SetUserInfo: Not found.");
                }
            }

            this.logger.Debug("SetUserInfo: Adding new passwrod credentials.");
            vault.Add(passwordCredential);
        }


        public bool Get(string resource, out string username)
        {
            username = null;

            PasswordVault vault = new PasswordVault();

            try
            {
                var list = vault.FindAllByResource(resource);
                if (list.Count > 0)
                {
                    this.logger.Debug("Get: Found password credentials. Count: {0}", list.Count);

                    username = list[0].UserName;
                    return true;
                }
            }
            catch (Exception exception)
            {
                if (((uint)exception.HResult) != 0x80070490)
                {
                    this.logger.Error(exception, "Exception while tried to GetUserInfo.");
                }
                else
                {
                    this.logger.Debug("GetUserInfo: Not found.");
                }
            }

            this.logger.Debug("Password credentials could not find.");
            return false;
        }

        public bool Get(string resource, out string username, out string password)
        {
            username = null;
            password = null;

            PasswordVault vault = new PasswordVault();

            try
            {
                var list = vault.FindAllByResource(resource);
                if (list.Count > 0)
                {
                    this.logger.Debug("Get: Found password credentials. Count: {0}", list.Count);
                    this.logger.Debug("Get: Retrieving password.");
                    list[0].RetrievePassword();
                    username = list[0].UserName;
                    password = list[0].Password;
                    return true;
                }
            }
            catch (Exception exception)
            {
                if (((uint)exception.HResult) != 0x80070490)
                {
                    this.logger.Error(exception, "Exception while tried to GetUserInfo.");
                }
                else
                {
                    this.logger.Debug("GetUserInfo: Not found.");
                }
            }

            this.logger.Debug("Password credentials could not find.");
            return false;
        }

        public void Delete(string resource)
        {
            PasswordVault vault = new PasswordVault();

            try
            {
                var all = vault.FindAllByResource(resource);
                foreach (var credential in all)
                {
                    this.logger.Debug("Remove old password credentials.");
                    vault.Remove(credential);
                }
            }
            catch (Exception exception)
            {
                if (((uint)exception.HResult) != 0x80070490)
                {
                    this.logger.Error(exception, "Exception while tried to ClearUserInfo.");
                }
                else
                {
                    this.logger.Debug("ClearUserInfo: Not found.");
                }
            }
        }
    }
}
