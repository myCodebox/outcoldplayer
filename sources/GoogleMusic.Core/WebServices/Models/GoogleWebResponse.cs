// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices.Models
{
    using System;
    using System.Collections.Generic;
    using System.Net;

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
                var responseStream = HttpWebResponse.GetResponseStream();
                using (var reader = new PlainLinesBodyReader(responseStream))
                {
                    return reader.GetValues();
                }
            }
            else
            {
                this.logger.Error("Unsupported content type '{0}'.", HttpWebResponse.ContentType);
                throw new NotSupportedException("Only 'text/plain' supported");
            }
        }
    }
}