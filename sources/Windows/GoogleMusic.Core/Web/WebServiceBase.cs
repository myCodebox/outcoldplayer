// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;

    public abstract class WebServiceBase
    {
        protected abstract ILogger Logger { get; }

        protected abstract HttpClient HttpClient { get; }

        protected async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead)
        {
            Debug.Assert(this.Logger != null, "this.Logger != null");
            Debug.Assert(this.HttpClient != null, "this.Logger != null");

            try
            {
                if (this.Logger.IsDebugEnabled)
                {
                    this.Logger.Debug("Send request ({0}): {1}.", requestMessage.Method, requestMessage.RequestUri);
                }

                HttpResponseMessage responseMessage = await this.HttpClient.SendAsync(requestMessage, completionOption);

                if (this.Logger.IsDebugEnabled)
                {
                    await this.Logger.LogResponseAsync(requestMessage.RequestUri.ToString(), responseMessage);
                }

                return responseMessage;
            }
            catch (Exception exception)
            {
                this.Logger.LogErrorException(exception);
                throw;
            }
        }
    }
}