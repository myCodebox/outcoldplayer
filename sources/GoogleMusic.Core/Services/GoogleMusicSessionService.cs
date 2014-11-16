// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;

    using OutcoldSolutions.GoogleMusic.Web;

    public class GoogleMusicSessionService : IGoogleMusicSessionService
    {
        private const string ContainerName = "GoogleMusicSessionService_UserSession";
        private const string SessionIdKey = "GoogleMusicSessionService_SessionId";
        private const string AuthKey = "GoogleMusicSessionService_Token";
        private const string CookiesKey = "GoogleMusicSessionService_Cookies";
        private const string CookiesFile = "cookies.cache";

        private readonly IDataProtectService dataProtectService;
        private readonly ISettingsService settingsService;
        private readonly ILogger logger;

        private UserSession userSession;

        private CookieContainerWrapper cookieContainer;
        private string auth;

        public GoogleMusicSessionService(ILogManager logManager, IDataProtectService dataProtectService, ISettingsService settingsService)
        {
            this.dataProtectService = dataProtectService;
            this.settingsService = settingsService;
            this.logger = logManager.CreateLogger("GoogleMusicSessionService");
        }

        public event EventHandler SessionCleared;

        public void InitializeCookieContainer(IEnumerable<Cookie> cookieCollection, string authValue)
        {
            if (cookieCollection == null)
            {
                throw new ArgumentNullException("cookieCollection");
            }

            this.cookieContainer = new CookieContainerWrapper();
            this.cookieContainer.AddCookies(cookieCollection);

            this.auth = authValue;
        }

        public CookieContainerWrapper GetCookieContainer()
        {
            return this.cookieContainer;
        }

        public string GetAuth()
        {
            return this.auth;
        }

        public UserSession GetSession()
        {
            if (this.userSession == null)
            {
                this.logger.Debug("User session is null, creating the new one.");

                this.userSession = new UserSession();
            }

            return this.userSession;
        }

        public async Task SaveCurrentSessionAsync()
        {
            if (this.userSession == null || this.cookieContainer == null)
            {
                this.logger.Debug("Current user session is null, ignoring save current session.");
                return;
            }

            var cookieCollection = this.cookieContainer.GetCookies();

            try
            {
                this.settingsService.SetValue(ContainerName, SessionIdKey, this.userSession.SessionId);
                this.settingsService.SetValue(ContainerName, AuthKey, this.auth);

                var cookies = cookieCollection.ToArray();

                string cookiesJson = JsonConvert.SerializeObject(cookies);

                string protectedCookies = await this.dataProtectService.ProtectStringAsync(cookiesJson);

                var storageFile = await ApplicationContext.ApplicationLocalFolder.CreateFileAsync(CookiesFile);
                await storageFile.WriteTextToFileAsync(protectedCookies);

                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Cookies and sessionId were saved. SessionId: {0}.", this.userSession.SessionId);
                    this.logger.Debug("---------------------------------");
                    this.logger.Debug("Saved cookies:");
                    this.logger.LogCookies(cookies);
                    this.logger.Debug("---------------------------------");
                }
            }
            catch (Exception exception)
            {
                this.logger.Error(exception, "Failed to save cookies");
            }
        }

        public async Task<IEnumerable<Cookie>> GetSavedCookiesAsync()
        {
            string protectedSerializedCookies = null;

            var storageFile = await ApplicationContext.ApplicationLocalFolder.CreateFileAsync(CookiesFile, CreationCollisionOption.OpenIfExists);
            if (storageFile != null)
            {
                this.logger.Debug("Trying to load cookies from file");
                protectedSerializedCookies = await storageFile.ReadFileTextContentAsync();
            }

            if (string.IsNullOrWhiteSpace(protectedSerializedCookies))
            {
                this.logger.Debug("Trying to load cookies from application settings");
                
                if (!this.settingsService.TryGetValue(ContainerName, CookiesKey, out protectedSerializedCookies))
                {
                    this.logger.Debug("GetSavedCookiesAsync: session container does not have value by {0} key.", CookiesKey);
                }
            }

            if (!string.IsNullOrWhiteSpace(protectedSerializedCookies))
            {
                try
                {
                    string unprotectedSerializedCookies =
                        await this.dataProtectService.UnprotectStringAsync(protectedSerializedCookies);

                    var cookiesArray = JsonConvert.DeserializeObject<Cookie[]>(unprotectedSerializedCookies);

                    if (cookiesArray != null)
                    {
                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug("---------------------------------");
                            this.logger.Debug("Loaded cookies:");
                            this.logger.LogCookies(cookiesArray);
                            this.logger.Debug("---------------------------------");
                        }

                        return cookiesArray;
                    }
                }
                catch (FormatException formatException)
                {
                    this.logger.Debug(formatException, "Could not deserialize cookies: {0}", protectedSerializedCookies);
                }
                catch (Exception e)
                {
                    this.logger.Error(
                        e, "GetSavedCookiesAsync: Cannot deserialize cookies. Protected cookies: '{0}'", protectedSerializedCookies);
                }
            }

            return null;
        }

        public void LoadSession()
        {
            if (this.userSession != null)
            {
                this.logger.Warning("LoadSession: User session object is not null. Ignoring.");
                return;
            }

            string sessionId;
            if (this.settingsService.TryGetValue(ContainerName, SessionIdKey, out sessionId))
            {
                this.userSession = new UserSession(sessionId);
            }

            this.auth = this.settingsService.GetValue<string>(ContainerName, AuthKey);
        }

        public async Task ClearSession(bool silent)
        {
            this.settingsService.RemoveValue(ContainerName, SessionIdKey);
            this.settingsService.RemoveValue(ContainerName, AuthKey);
            this.userSession = null;
            this.cookieContainer = null;
            try
            {
                var file = await ApplicationContext.ApplicationLocalFolder.GetFileAsync(CookiesFile);
                if (file != null)
                {
                    await file.DeleteAsync();
                }
            }
            catch (Exception exp)
            {
                this.logger.Debug(exp, "Could not detele cookies file");
            }

            if (!silent)
            {
                this.RaiseSessionCleared();
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