// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Security.Credentials;

    public class UserDataStorage : IUserDataStorage
    {
        private const string GoogleAccountsResource = "OutcoldSolutions.GoogleMusic";

        private CookieCollection storedCookieCollection;
        private Uri storedUrl;

        public void SaveUserInfo(UserInfo userInfo)
        {
            var passwordCredential = new PasswordCredential(GoogleAccountsResource, userInfo.Email, userInfo.Password);
            PasswordVault vault = new PasswordVault();

            // Remove old
            try
            {
                var all = vault.FindAllByResource(GoogleAccountsResource);
                foreach (var credential in all)
                {
                    vault.Remove(credential);
                }
            }
            catch (Exception)
            {
            }
            
            vault.Add(passwordCredential);
        }

        public UserInfo GetUserInfo()
        {
            PasswordVault vault = new PasswordVault();

            try
            {
                var list = vault.FindAllByResource(GoogleAccountsResource);
                if (list.Count > 0)
                {
                    list[0].RetrievePassword();
                    return new UserInfo(list[0].UserName, list[0].Password);
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        public void SaveCookies(Uri url, CookieCollection cookieCollection)
        {
            this.storedUrl = url;
            this.storedCookieCollection = cookieCollection;
        }

        public CookieContainer GetCookieContainer()
        {
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(this.storedUrl, this.storedCookieCollection);
            return cookieContainer;
        }
    }
}