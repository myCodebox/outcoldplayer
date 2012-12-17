// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    public class GoogleWebResponse
    {
        private readonly ILogger logger;

        public GoogleWebResponse(ILogManager logManager, HttpWebResponse httpWebResponse)
        {
            this.logger = logManager.CreateLogger("GoogleWebResponse");
            this.HttpWebResponse = httpWebResponse;
        }

        public HttpWebResponse HttpWebResponse { get; private set; }

        public IDictionary<string, string> GetAsPlainLines()
        {
            if (HttpWebResponse.ContentType.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
            {
                using (var responseStream = HttpWebResponse.GetResponseStream())
                {
                    using (var reader = new PlainLinesBodyReader(responseStream))
                    {
                        return reader.GetValues();
                    }
                }
            }
            else
            {
                this.logger.Error("Unsupported content type '{0}'.", HttpWebResponse.ContentType);
                throw new NotSupportedException("Only 'text/plain' supported");
            }
        }

        public TType GetAsJsonObject<TType>() where TType : class
        {
            using (var responseStream = HttpWebResponse.GetResponseStream())
            {
                using (var reader = new StreamReader(responseStream))
                {
                    var body = reader.ReadToEnd();

                    try
                    {
                        return JsonConvert.DeserializeObject<TType>(body);
                    }
                    catch (Exception e)
                    {
                        this.logger.Error("Canot deserialize json data '{0}'", body);
                        this.logger.LogException(e);
                    }
                }
            }

            return null;
        }
    }
}