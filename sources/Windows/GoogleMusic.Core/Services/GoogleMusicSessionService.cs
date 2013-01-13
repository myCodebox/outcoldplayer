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

    using Windows.Storage;

    public class GoogleMusicSessionService : IGoogleMusicSessionService
    {
        private const string ContainerName = "UserSession";
        private const string SessionIdKey = "SessionId";
        private const string CookiesKey = "Cookies";

        private readonly ILogger logger;

        private UserSession userSession;

        public GoogleMusicSessionService(ILogManager logManager)
        {
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

        public void SaveCurrentSession(CookieCollection cookieCollection)
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

                var cookies = JsonConvert.SerializeObject(cookieCollection.Cast<Cookie>().ToArray());
                applicationDataContainer.Values[CookiesKey] = cookies;

                this.logger.Debug("Cookies and sessionId were saved. SessionId: {0}. Cookies: {1}.", this.userSession.SessionId, cookies);
            }
            else
            {
                this.logger.Error("SaveCurrentSession: GetSessionContainer returns null.");
            }
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

        public CookieCollection GetSavedCookies()
        {
            var applicationDataContainer = this.GetSessionContainer();
            if (applicationDataContainer != null)
            {
                object cookies;
                if (applicationDataContainer.Values.TryGetValue(CookiesKey, out cookies))
                {
                    try
                    {
                        string serializedCookies = Convert.ToString(cookies);
                        this.logger.Debug("GetSavedCookies: serialized saved cookies: {0}.", serializedCookies);

                        var cookiesArray = JsonConvert.DeserializeObject<Cookie[]>(serializedCookies);

                        if (cookiesArray != null)
                        {
                            CookieCollection result = new CookieCollection();
                            foreach (var cookie in cookiesArray)
                            {
                                result.Add(cookie);
                            }

                            return result;
                        }
                    }
                    catch (Exception e)
                    {
                        this.logger.Error("GetSavedCookies: Cannot deserialize cookies");
                        this.logger.LogErrorException(e);
                    }
                }
                else
                {
                    this.logger.Debug("GetSavedCookies: session container does not have value by {0} key.", CookiesKey);
                }
            }
            else
            {
                this.logger.Debug("GetSavedCookies: session container is null. Cannot get cookies.");
            }

            return null;
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