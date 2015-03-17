// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using System;
    using System.Net;
    using System.Net.Http;

    public static class HttpClientHandlerExtensionsEx
    {
        public static HttpClientHandler GetWithProxy(
            ILogger logger, 
            ISettingsService settingsService)
        {
            return SetProxy(new HttpClientHandler(), logger, settingsService);
        }

        public static HttpClientHandler SetProxy(
            HttpClientHandler handler,
            ILogger logger,
            ISettingsService settingsService)
        {
            try
            {
                if (handler.Proxy != null && settingsService.GetIsProxySupport())
                {
                    handler.Proxy.Credentials = CredentialCache.DefaultCredentials;
                }
            }
            catch (Exception e)
            {
                logger.Warning(e, "Could not setup proxy");
            }

            return handler;
        }
    }
}
