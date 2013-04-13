// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Net;

    public class WebRequestException : Exception
    {
        public WebRequestException(HttpStatusCode statusCode)
        {
            this.StatusCode = statusCode;
        }

        public WebRequestException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            this.StatusCode = statusCode;
        }

        public WebRequestException(string message, Exception innerException, HttpStatusCode statusCode)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; private set; }
    }
}