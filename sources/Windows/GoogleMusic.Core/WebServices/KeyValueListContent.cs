// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    public class KeyValueListContent : IDisposable
    {
        private readonly StreamReader streamReader;

        public KeyValueListContent(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.streamReader = new StreamReader(stream);
        }

        ~KeyValueListContent()
        {
            this.Dispose(disposing: false);
        }

        public async Task<IDictionary<string, string>> GetValuesAsync()
        {
            var responseValues = new Dictionary<string, string>();

            while (!this.streamReader.EndOfStream)
            {
                var responseLine = await this.streamReader.ReadLineAsync();
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