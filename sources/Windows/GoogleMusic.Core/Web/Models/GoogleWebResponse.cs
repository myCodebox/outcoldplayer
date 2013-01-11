// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    using System;
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

        public TType GetAsJsonObject<TType>() where TType : class
        {
            if (this.HttpWebResponse != null)
            {
                using (var responseStream = this.HttpWebResponse.GetResponseStream())
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