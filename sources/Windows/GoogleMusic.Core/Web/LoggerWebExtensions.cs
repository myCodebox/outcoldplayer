// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;

#if !DEBUG
    using Windows.Security.Cryptography;
    using Windows.Security.Cryptography.Core;
    using Windows.Storage.Streams;
#endif

    public static class LoggerWebExtensions
    {
        public static void LogRequest(
            this ILogger @this,
            HttpMethod method,
            string requestUrl, 
            CookieCollection cookieCollection = null,
            IDictionary<string, string> formData = null)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            if (@this.IsDebugEnabled)
            {
                var log = new StringBuilder();

                log.AppendFormat("{0} REQUEST: {1}.", method, requestUrl);
                log.AppendLine();

                if (formData != null)
                {
                    log.AppendLine("    FORMDATA: ");

                    foreach (var argument in formData)
                    {
                        log.AppendFormat("        {0}={1}", argument.Key, argument.Value);
                        log.AppendLine();
                    }
                }

                LogCookies(log, cookieCollection);

                log.AppendLine();

                @this.Debug(log.ToString());
            }
        }

        public static void LogCookies(this ILogger @this, CookieCollection cookieCollection)
        {
            if (@this.IsDebugEnabled)
            {
                if (cookieCollection != null)
                {
                    var log = new StringBuilder();
                    LogCookies(log, cookieCollection);

                    @this.Debug(log.ToString());
                }
            }
        }

        public static async Task LogResponseAsync(
            this ILogger @this, 
            string requestUrl, 
            HttpResponseMessage httpResponseMessage)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            if (httpResponseMessage == null)
            {
                throw new ArgumentNullException("httpResponseMessage");
            }

            if (@this.IsDebugEnabled)
            {
                var log = new StringBuilder();

                log.AppendFormat("Request '{0}' completed, Status code: {1}.", requestUrl, httpResponseMessage.StatusCode);
                log.AppendLine();
                log.AppendFormat("RequestUri: {0}.", httpResponseMessage.RequestMessage.RequestUri);
                log.AppendLine();

                log.AppendLine("    RESPONSE HEADERS: ");

                foreach (var httpResponseHeader in httpResponseMessage.Headers)
                {
                    log.AppendFormat("        {0}={1}", httpResponseHeader.Key, string.Join("&&&", httpResponseHeader.Value));
                    log.AppendLine();
                }

                if (httpResponseMessage.Content != null)
                {
                    log.AppendLine("    RESPONSE CONTENT HEADERS: ");

                    foreach (var header in httpResponseMessage.Content.Headers)
                    {
                        log.AppendFormat("        {0}={1}", header.Key, string.Join("&&&", header.Value));
                        log.AppendLine();
                    }

                    if (httpResponseMessage.Content.IsPlainText()
                        || httpResponseMessage.Content.IsHtmlText()
                        || httpResponseMessage.Content.IsJson())
                    {
                        var content = await httpResponseMessage.Content.ReadAsStringAsync();

                        log.AppendFormat("    RESPONSE CONTENT:{0}{1}", Environment.NewLine, content.Substring(0, Math.Min(4096, content.Length)));
                        log.AppendLine();
                        log.AppendFormat("    RESPONSE ENDCONTENT.");
                        log.AppendLine();
                    }
                }
                else
                {
                    log.AppendLine("CONTENT is null.");
                }

                log.AppendLine();

                @this.Debug(log.ToString());
            }
        }

        private static void LogCookies(StringBuilder log, CookieCollection cookieCollection)
        {
            if (cookieCollection != null)
            {
                log.AppendFormat("    COOKIES({0}):", cookieCollection.Count);
                log.AppendLine();

                foreach (Cookie cookieLog in cookieCollection)
                {
#if DEBUG
                    log.AppendFormat("        {0}={1}, Expires={2}", cookieLog.Name, cookieLog.Value, cookieLog.Expires);
                    log.AppendLine();
#else

                    HashAlgorithmProvider hashProvider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
                    IBuffer hash =
                        hashProvider.HashData(
                            CryptographicBuffer.ConvertStringToBinary(cookieLog.Value, BinaryStringEncoding.Utf8));
                    string hashValue = CryptographicBuffer.EncodeToBase64String(hash);

                    log.AppendFormat("        {0}={{MD5_VALUE_HASH}}{1}, Expires={2}", cookieLog.Name, hashValue, cookieLog.Expires);
                    log.AppendLine();
#endif
                }
            }
        }
    }
}