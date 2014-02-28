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

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Storage;

    using OutcoldSolutions.GoogleMusic.Web;

    using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;

    public class GoogleMusicSessionService : IGoogleMusicSessionService
    {
        private const string ContainerName = "GoogleMusicSessionService_UserSession";
        private const string SessionIdKey = "GoogleMusicSessionService_SessionId";
        private const string AuthKey = "GoogleMusicSessionService_Token";
        private const string CookiesKey = "GoogleMusicSessionService_Cookies";
        private const string CookiesFile = "cookies.cache";

        private readonly IDataProtectService dataProtectService;
        private readonly ILogger logger;

        private UserSession userSession;

        private CookieContainerWrapper cookieContainer;
        private string auth;

        public GoogleMusicSessionService(ILogManager logManager, IDataProtectService dataProtectService)
        {
            this.dataProtectService = dataProtectService;
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

            var applicationDataContainer = this.GetSessionContainer();
            if (applicationDataContainer != null)
            {
                try
                {
                    applicationDataContainer.Values[SessionIdKey] = this.userSession.SessionId;
                    applicationDataContainer.Values[AuthKey] = this.auth;

                    var cookies = cookieCollection.ToArray();

                    string cookiesJson = JsonConvert.SerializeObject(cookies);
                    
                    string protectedCookies = await this.dataProtectService.ProtectStringAsync(cookiesJson);
                    var storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(CookiesFile, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(storageFile, protectedCookies, UnicodeEncoding.Utf8);

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
            else
            {
                this.logger.Error("SaveCurrentSessionAsync: GetSessionContainer returns null.");
            }
        }

        public async Task<IEnumerable<Cookie>> GetSavedCookiesAsync()
        {
            string protectedSerializedCookies = null;

            var storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(CookiesFile, CreationCollisionOption.OpenIfExists);
            if (storageFile != null)
            {
                this.logger.Debug("Trying to load cookies from file");
                protectedSerializedCookies = await FileIO.ReadTextAsync(storageFile, UnicodeEncoding.Utf8);
            }

            if (string.IsNullOrWhiteSpace(protectedSerializedCookies))
            {
                this.logger.Debug("Trying to load cookies from application settings");
                var applicationDataContainer = this.GetSessionContainer();
                if (applicationDataContainer != null)
                {
                    object cookies;
                    if (applicationDataContainer.Values.TryGetValue(CookiesKey, out cookies) && cookies is string)
                    {
                        protectedSerializedCookies = Convert.ToString(cookies);
                    }
                    else
                    {
                        this.logger.Debug("GetSavedCookiesAsync: session container does not have value by {0} key.", CookiesKey);
                    }
                }
                else
                {
                    this.logger.Debug("GetSavedCookiesAsync: session container is null. Cannot get cookies.");
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

            var applicationDataContainer = this.GetSessionContainer();
            if (applicationDataContainer != null)
            {
                object sessionIdObject;
                if (applicationDataContainer.Values.TryGetValue(SessionIdKey, out sessionIdObject))
                {
                    try
                    {
                        var sessionId = Convert.ToString(sessionIdObject);

                        this.logger.Debug("LoadSession: user session restored - {0}.", sessionId);

                        this.userSession = new UserSession(sessionId);
                    }
                    catch (Exception e)
                    {
                        this.logger.Error(e, "LoadSession: cannot deserialize sessionId.");
                    }
                }

                object tokenObject;
                if (applicationDataContainer.Values.TryGetValue(AuthKey, out tokenObject))
                {
                    try
                    {
                        this.auth = Convert.ToString(tokenObject);

                        this.logger.Debug("LoadSession: auth.");
                    }
                    catch (Exception e)
                    {
                        this.logger.Error(e, "LoadSession: cannot deserialize auth.");
                    }
                }
            }
            else
            {
                this.logger.Error("LoadSession: GetSessionContainer returns null.");
            }
        }

        public async Task ClearSession()
        {
            var sessionContainer = this.GetSessionContainer();
            if (sessionContainer != null)
            {
                sessionContainer.Values.Clear();
                this.userSession = null;
                this.cookieContainer = null;
                try
                {
                    var file = await ApplicationData.Current.LocalFolder.GetFileAsync(CookiesFile);
                    if (file != null)
                    {
                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                }
                catch (Exception exp)
                {
                    this.logger.Debug(exp, "Could not detele cookies file");
                }

                this.RaiseSessionCleared();
            }
            else
            {
                this.logger.Error("ClearSession: GetSessionContainer returns null.");
            }
        }

        private ApplicationDataContainer GetSessionContainer()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer userSessionContainer;
            if (!localSettings.Containers.TryGetValue(ContainerName, out userSessionContainer))
            {
                this.logger.Debug("Local settings does not have {0} container. Creating the new one.", ContainerName);
                userSessionContainer = localSettings.CreateContainer(ContainerName, ApplicationDataCreateDisposition.Always);
            }
           
            return userSessionContainer;
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