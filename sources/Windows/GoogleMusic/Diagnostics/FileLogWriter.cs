// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Windows.Storage;

    public class FileLogWriter : ILogWriter, IDisposable
    {
        private readonly object locker = new object();

        private StreamWriter writer;

        public bool IsEnabled
        {
            get
            {
                return this.writer != null;
            }

            set
            {
                if (value && this.writer == null)
                {
                    this.EnableLogging();
                }
                else if (!value && this.writer != null)
                {
                    this.ClearStream();
                }
            }
        }

        public void Dispose()
        {
            lock (this.locker)
            {
                if (this.writer != null)
                {
                    this.ClearStream();
                }
            }
        }

        public void Log(string level, string context, string message, params object[] parameters)
        {
            lock (this.locker)
            {
                if (this.writer != null)
                {
                    this.writer.WriteLine("{0}::: {1} --- {2}", level, context, string.Format(message, parameters));
                    this.writer.Flush();
                }
            }
        }

        private void EnableLogging()
        {
            var enableLoggingAsync = this.EnableLoggingAsync();
            enableLoggingAsync.Wait();
            if (enableLoggingAsync.IsCompleted)
            {
                lock (this.locker)
                {
                    this.writer = enableLoggingAsync.Result;   
                }
            }
        }

        private void ClearStream()
        {
            lock (this.locker)
            {
                this.writer.Flush();
                this.writer.Dispose();
                this.writer = null;
            }
        }

        private async Task<StreamWriter> EnableLoggingAsync()
        {
            var storageFile =
                await ApplicationData.Current.LocalFolder.CreateFileAsync(string.Format("{0:yyyy-MM-dd-HH-mm-ss-ffff}.log", DateTime.Now)).AsTask();

            var stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite).AsTask();

            return new StreamWriter(stream.AsStream());
        }
    }
}
