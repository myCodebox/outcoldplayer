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
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    internal static class HttpContentExtensions
    {
        internal static async Task<Dictionary<string, string>> ReadAsDictionaryAsync(this HttpContent @this)
        {
            if (!@this.Headers.ContentType.IsPlainText())
            {
                throw new NotSupportedException("ReadAsDictionaryAsync supports only text/plain content.");
            }

            var responseValues = new Dictionary<string, string>();

            using (var stream = await @this.ReadAsStreamAsync())
            {
                using (var streamReader = new StreamReader(stream))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var responseLine = await streamReader.ReadLineAsync();
                        var firstEqual = responseLine.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                        if (firstEqual > 0)
                        {
                            string name = responseLine.Substring(0, firstEqual);
                            string value = responseLine.Substring(
                                firstEqual + 1, responseLine.Length - (firstEqual + 1));
                            responseValues.Add(name, WebUtility.UrlDecode(value));
                        }
                        else
                        {
                            responseValues.Add(responseLine, string.Empty);
                        }
                    }
                }
            }

            return responseValues;
        }

        internal static async Task<TResult> ReadAsJsonObject<TResult>(this HttpContent @this)
        {
            if (!@this.Headers.ContentType.IsPlainText())
            {
                throw new NotSupportedException("ReadAsJsonObject supports only text/plain content.");
            }

            return JsonConvert.DeserializeObject<TResult>(await @this.ReadAsStringAsync());
        }
    }
}