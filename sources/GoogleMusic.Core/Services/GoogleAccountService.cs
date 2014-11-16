// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Shell;

    public class GoogleAccountService : IGoogleAccountService
    {
        private readonly ISecureStorageService secureStorageService;
        private const string GoogleAccountsResource = "OutcoldSolutions.GoogleMusic";

        private readonly ILogger logger;

        public GoogleAccountService(ILogManager logManager, ISecureStorageService secureStorageService)
        {
            this.secureStorageService = secureStorageService;
            this.logger = logManager.CreateLogger("GoogleAccountService");
        }

        public void SetUserInfo(UserInfo info)
        {
            this.logger.Debug("SetUserInfo. Is null {0}.", info == null);
            
            if (info != null && info.RememberAccount)
            {
                this.secureStorageService.Save(GoogleAccountsResource, info.Email, info.Password);
            }
        }

        public UserInfo GetUserInfo(bool retrievePassword = false)
        {
            this.logger.Debug("GetUserInfo");

            string email = null;
            
            if (retrievePassword)
            {
                string password = null;

                if (this.secureStorageService.Get(GoogleAccountsResource, out email, out password))
                {
                    return new UserInfo(email, password);
                } 
            }
            else
            {
                if (this.secureStorageService.Get(GoogleAccountsResource, out email))
                {
                    return new UserInfo(email, null);
                } 
            }

            return null;
        }

        public void ClearUserInfo()
        {
            this.logger.Debug("ClearUserInfo");

            this.secureStorageService.Delete(GoogleAccountsResource);
        }
    }
}