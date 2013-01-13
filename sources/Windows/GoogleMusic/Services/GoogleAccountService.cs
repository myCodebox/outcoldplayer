// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Linq;
    using System.Net;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Security.Credentials;
    using Windows.Storage;

    public class GoogleAccountService : IGoogleAccountService
    {
        private const string GoogleAccountsResource = "OutcoldSolutions.GoogleMusic";

        private readonly ILogger logger;

        private UserSession userSession;
        private UserInfo userInfo;

        public GoogleAccountService(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("GoogleAccountService");
        }

        public event EventHandler SessionCleared;

        public void SetUserInfo(UserInfo info)
        {
            this.logger.Debug("SetUserInfo. Is null {0}.", info == null);
            this.userInfo = info;

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
        }

        public UserInfo GetUserInfo()
        {
            this.logger.Debug("GetUserInfo");

            if (this.userInfo != null)
            {
                this.logger.Debug("User info set before");
                return this.userInfo;
            }

            PasswordVault vault = new PasswordVault();

            try
            {
                var list = vault.FindAllByResource(GoogleAccountsResource);
                if (list.Count > 0)
                {
                    this.logger.Debug("Found password credentials. Count: {0}", list.Count);
                    list[0].RetrievePassword();
                    var info = new UserInfo(list[0].UserName, list[0].Password) { RememberAccount = true };
                    this.SetUserInfo(info);
                    return this.userInfo = info;
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

            this.userInfo = null;

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
            this.userSession = session;
        }

        public UserSession GetUserSession()
        {
            return this.userSession;
        }

        public void ClearSession()
        {
            this.logger.Debug("ClearSession: clearing session.");

            if (this.userSession != null)
            {
                this.userInfo = null;
                this.userSession = null;

                var localSettings = ApplicationData.Current.LocalSettings;
                if (localSettings.Containers.ContainsKey("UserSession"))
                {
                    localSettings.DeleteContainer("UserSession");
                }

                this.RaiseSessionCleared();

                this.logger.Debug("ClearSession: session was cleared.");
            }
            else
            {
                this.logger.Warning("ClearSession: Current session is null already.");
            }
        }

        public void SaveCurrentSession(CookieCollection cookieCollection)
        {
            if (this.userSession != null)
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                
                ApplicationDataContainer userSessionContainer;
                if (!localSettings.Containers.TryGetValue("UserSession", out userSessionContainer))
                {
                    userSessionContainer = localSettings.CreateContainer("UserSession", ApplicationDataCreateDisposition.Always);
                }

                foreach (var val in userSessionContainer.Values)
                {
                    userSessionContainer.Values.Remove(val.Key);
                }

                userSessionContainer.Values["UserSessionId"] = this.userSession.SessionId;
                userSessionContainer.Values["Cookies"] = JsonConvert.SerializeObject(cookieCollection.Cast<Cookie>().ToArray());
            }
        }

        private void RaiseSessionCleared()
        {
            var handler = this.SessionCleared;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}