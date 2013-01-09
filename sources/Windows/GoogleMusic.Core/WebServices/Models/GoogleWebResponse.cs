// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;

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

        public GoogleWebResponse(ILogManager logManager, HttpResponseMessage httpResponseMessage)
        {
            this.logger = logManager.CreateLogger("GoogleWebResponse");
            this.HttpResponseMessage = httpResponseMessage;
        }

        public HttpWebResponse HttpWebResponse { get; private set; }

        public HttpResponseMessage HttpResponseMessage { get; private set; }

        public IDictionary<string, string> GetAsPlainLines()
        {
            if (this.HttpWebResponse != null)
            {
                if (this.HttpWebResponse.ContentType.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
                {
                    using (var responseStream = this.HttpWebResponse.GetResponseStream())
                    {
                        using (var reader = new PlainLinesBodyReader(responseStream))
                        {
                            return reader.GetValues();
                        }
                    }
                }
                else
                {
                    this.logger.Error("Unsupported content type '{0}'.", this.HttpWebResponse.ContentType);
                    throw new NotSupportedException("Only 'text/plain' supported");
                }
            }
            else if (this.HttpResponseMessage != null)
            {
                if (this.HttpResponseMessage.Content.Headers.ContentType.MediaType.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
                {
                    var readAsStreamAsync = this.HttpResponseMessage.Content.ReadAsStreamAsync();
                    readAsStreamAsync.Wait();

                    using (var responseStream = readAsStreamAsync.Result)
                    {
                        using (var reader = new PlainLinesBodyReader(responseStream))
                        {
                            return reader.GetValues();
                        }
                    }
                }
                else
                {
                    this.logger.Error("Unsupported content type '{0}'.", this.HttpResponseMessage.Content.Headers.ContentType.MediaType);
                    throw new NotSupportedException("Only 'text/plain' supported");
                }
            }

            return null;
        }

        public TType GetAsJsonObject<TType>() where TType : class
        {
            if (this.HttpWebResponse != null)
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
                            this.logger.Error("Canot deserialize json data: '{0}'", body);
                            this.logger.LogErrorException(e);
                        }
                    }
                }
            }
            else if (this.HttpResponseMessage != null)
            {
                var readAsStringAsync = this.HttpResponseMessage.Content.ReadAsStringAsync();
                readAsStringAsync.Wait();

                try
                {
                    return JsonConvert.DeserializeObject<TType>(readAsStringAsync.Result);
                }
                catch (Exception e)
                {
                    this.logger.Error("Canot deserialize json data: '{0}'", readAsStringAsync.Result);
                    this.logger.LogErrorException(e);
                }
            }

            return null;
        }
    }
}