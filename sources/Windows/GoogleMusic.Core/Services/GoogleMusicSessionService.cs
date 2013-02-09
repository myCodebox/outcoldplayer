// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web;

    using Windows.Storage;

    public class GoogleMusicSessionService : IGoogleMusicSessionService
    {
        private const string ContainerName = "UserSession";
        private const string SessionIdKey = "SessionId";
        private const string CookiesKey = "Cookies";

        private readonly IDataProtectService dataProtectService;
        private readonly ILogger logger;

        private UserSession userSession;

        public GoogleMusicSessionService(ILogManager logManager, IDataProtectService dataProtectService)
        {
            this.dataProtectService = dataProtectService;
            this.logger = logManager.CreateLogger("GoogleMusicSessionService");
        }

        public event EventHandler SessionCleared;

        public UserSession GetSession()
        {
            if (this.userSession == null)
            {
                this.logger.Debug("User session is null, creating the new one.");

                this.userSession = new UserSession();
            }

            return this.userSession;
        }

        public async Task SaveCurrentSessionAsync(CookieCollection cookieCollection)
        {
            if (cookieCollection == null)
            {
                throw new ArgumentNullException("cookieCollection");
            }

            if (this.userSession == null)
            {
                this.logger.Debug("Current user session is null, ignoring save current session.");
                return;
            }

            var applicationDataContainer = this.GetSessionContainer();
            if (applicationDataContainer != null)
            {
                applicationDataContainer.Values[SessionIdKey] = this.userSession.SessionId;

                string cookies = JsonConvert.SerializeObject(cookieCollection.Cast<Cookie>().ToArray());

                string protectedCookies = await this.dataProtectService.ProtectStringAsync(cookies);

                applicationDataContainer.Values[CookiesKey] = protectedCookies;
                
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Cookies and sessionId were saved. SessionId: {0}.", this.userSession.SessionId);
                    this.logger.Debug("---------------------------------");
                    this.logger.Debug("Saved cookies:");
                    this.logger.LogCookies(cookieCollection);
                    this.logger.Debug("---------------------------------");
                }
            }
            else
            {
                this.logger.Error("SaveCurrentSessionAsync: GetSessionContainer returns null.");
            }
        }

        public async Task<CookieCollection> GetSavedCookiesAsync()
        {
            var applicationDataContainer = this.GetSessionContainer();
            if (applicationDataContainer != null)
            {
                object cookies;
                if (applicationDataContainer.Values.TryGetValue(CookiesKey, out cookies))
                {
                    try
                    {
                        string unprotectedSerializedCookies = await this.dataProtectService.UnprotectStringAsync(Convert.ToString(cookies));

                        var cookiesArray = JsonConvert.DeserializeObject<Cookie[]>(unprotectedSerializedCookies);

                        if (cookiesArray != null)
                        {
                            CookieCollection result = new CookieCollection();
                            foreach (var cookie in cookiesArray)
                            {
                                result.Add(cookie);
                            }

                            if (this.logger.IsDebugEnabled)
                            {
                                this.logger.Debug("---------------------------------");
                                this.logger.Debug("Loaded cookies:");
                                this.logger.LogCookies(result);
                                this.logger.Debug("---------------------------------");
                            }

                            return result;
                        }
                    }
                    catch (Exception e)
                    {
                        this.logger.Error("GetSavedCookiesAsync: Cannot deserialize cookies");
                        this.logger.LogErrorException(e);
                    }
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
                        this.logger.Error("LoadSession: cannot deserialize sessionId.");
                        this.logger.LogErrorException(e);
                    }
                }
            }
            else
            {
                this.logger.Error("LoadSession: GetSessionContainer returns null.");
            }
        }

        public void ClearSession()
        {
            var sessionContainer = this.GetSessionContainer();
            if (sessionContainer != null)
            {
                sessionContainer.Values.Clear();
                this.userSession = null;

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