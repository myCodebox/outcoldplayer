// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class FormBuilder : IDisposable
    {
        private readonly string boundary = "----------" + DateTime.Now.Ticks.ToString("x");

        private readonly MemoryStream ms;
        private readonly StreamWriter writer;

        public FormBuilder()
        {
            this.ms = new MemoryStream();
            this.writer = new StreamWriter(this.ms, Encoding.UTF8);
        }

        ~FormBuilder()
        {
            this.Dispose(disposing: false);
        }

        public static FormBuilder Empty
        {
            get
            {
                FormBuilder b = new FormBuilder();
                b.Close();
                return b;
            }
        }

        public string ContentType
        {
            get
            {
                return "multipart/form-data; boundary=" + this.boundary;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void AddFields(Dictionary<string, string> fields)
        {
            foreach (KeyValuePair<string, string> key in fields)
            {
                this.AddField(key.Key, key.Value);
            }
        }

        public void AddField(string key, string value)
        {
            this.writer.Write("\r\n--" + this.boundary + "\r\n");
            this.writer.Write(string.Format("Content-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}", key, value));
        }

        public void Close()
        {
            this.WriteBoundary();
        }

        public byte[] GetBytes()
        {
            this.writer.Flush();
            return this.ms.ToArray();
        }

        private void WriteBoundary()
        {
            this.writer.Write("\r\n--" + this.boundary + "--\r\n");
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.writer.Dispose();
                this.ms.Dispose();
            }
        }
    }
}