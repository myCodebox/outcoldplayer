// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    public class RequestBodyWriter : IDisposable
    {
        private readonly StreamWriter streamWriter;
        private bool empty = true;

        public RequestBodyWriter(Stream requestStream)
        {
            if (requestStream == null)
            {
                throw new ArgumentNullException("requestStream");
            }

            this.streamWriter = new StreamWriter(requestStream, Encoding.UTF8);
        }

        ~RequestBodyWriter()
        {
            this.Dispose(disposing: false);
        }

        public void Add(string name, string value)
        {
            if (!this.empty)
            {
                this.streamWriter.Write("&");
            }

            this.streamWriter.Write("{0}={1}", name, WebUtility.UrlEncode(value));

            this.empty = false;
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.streamWriter.Dispose();
            }
        }
    }
}