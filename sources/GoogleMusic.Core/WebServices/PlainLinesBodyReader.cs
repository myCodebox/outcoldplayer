// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    public class PlainLinesBodyReader : IDisposable
    {
        private readonly StreamReader streamReader;

        public PlainLinesBodyReader(Stream responseSteam)
        {
            if (responseSteam == null)
            {
                throw new ArgumentNullException("responseSteam");
            }

            this.streamReader = new StreamReader(responseSteam);
        }

        ~PlainLinesBodyReader()
        {
            this.Dispose(disposing: false);
        }

        public IDictionary<string, string> GetValues()
        {
            var responseValues = new Dictionary<string, string>();

            while (!this.streamReader.EndOfStream)
            {
                var responseLine = this.streamReader.ReadLine();
                var firstEqual = responseLine.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                if (firstEqual > 0)
                {
                    string name = responseLine.Substring(0, firstEqual);
                    string value = responseLine.Substring(firstEqual + 1, responseLine.Length - (firstEqual + 1));
                    responseValues.Add(name, WebUtility.UrlDecode(value));
                }
                else
                {
                    responseValues.Add(responseLine, string.Empty);
                }
            }

            return responseValues;
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
                this.streamReader.Dispose();
            }
        }
    }
}