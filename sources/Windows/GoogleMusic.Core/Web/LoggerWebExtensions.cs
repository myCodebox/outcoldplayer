// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    using Windows.Security.Cryptography;
    using Windows.Security.Cryptography.Core;
    using Windows.Storage.Streams;

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
                @this.Debug("-----------------------");

                @this.Debug("{0} REQUEST: {1}.", method, requestUrl);

                if (formData != null)
                {
                    @this.Debug("    FORMDATA: ");

                    foreach (var argument in formData)
                    {
                        @this.Debug("        {0}={1}", argument.Key, argument.Value);
                    }
                }

                if (cookieCollection != null)
                {
                    @this.Debug("    COOKIES({0}):", cookieCollection.Count);

                    foreach (Cookie cookieLog in cookieCollection)
                    {
                        HashAlgorithmProvider hashProvider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
                        IBuffer hash = hashProvider.HashData(CryptographicBuffer.ConvertStringToBinary(cookieLog.Value, BinaryStringEncoding.Utf8));
                        string hashValue = CryptographicBuffer.EncodeToBase64String(hash);

                        @this.Debug("        {0}={{MD5_VALUE_HASH}}{1}", cookieLog.Name, hashValue);
                    }
                }

                @this.Debug("-----------------------");
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
                @this.Debug("------------------------------");
                @this.Debug("Request '{0}' completed, Status code: {1}.", requestUrl, httpResponseMessage.StatusCode);
                @this.Debug("RequestUri: {0}.", httpResponseMessage.RequestMessage.RequestUri);

                @this.Debug("    RESPONSE HEADERS: ");

                foreach (var httpResponseHeader in httpResponseMessage.Headers)
                {
                    @this.Debug("        {0}={1}", httpResponseHeader.Key, string.Join("&&&", httpResponseHeader.Value));
                }

                if (httpResponseMessage.Content != null)
                {
                    @this.Debug("    RESPONSE CONTENT HEADERS: ");

                    foreach (var header in httpResponseMessage.Content.Headers)
                    {
                        @this.Debug("        {0}={1}", header.Key, string.Join("&&&", header.Value));
                    }

                    if (httpResponseMessage.Content.IsPlainText()
                        || httpResponseMessage.Content.IsHtmlText()
                        || httpResponseMessage.Content.IsJson())
                    {
                        using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync())
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                char[] buffer = new char[4096];
                                var read = await reader.ReadAsync(buffer, 0, buffer.Length);

                                if (read > 0)
                                {
                                    var bodyData = new StringBuilder();
                                    bodyData.Append(buffer, 0, read);

                                    @this.Debug("    RESPONSE CONTENT:{0}{1}", Environment.NewLine, bodyData);
                                    @this.Debug("    RESPONSE ENDCONTENT.");
                                }
                            }
                        }
                    }
                }
                else
                {
                    @this.Debug("CONTENT is null.");
                }

                @this.Debug("-----------------------------");
            }
        }
    }
}