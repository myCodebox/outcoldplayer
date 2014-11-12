// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;

    using TaskExtensions = OutcoldSolutions.GoogleMusic.TaskExtensions;

    /// <summary>
    /// The file log writer.
    /// </summary>
    public class FileLogWriter : ILogWriter, IDisposable
    {
        private readonly object locker = new object();

        private StreamWriter writer;

        /// <inheritdoc />
        public bool IsEnabled
        {
            get
            {
                return true;
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void Log(DateTime dateTime, LogLevel level, string context, string message, params object[] parameters)
        {
            this.Log(dateTime, level, context, null, message, parameters);
        }

        /// <inheritdoc />
        public void Log(DateTime dateTime, LogLevel level, string context, Exception exception, string message, params object[] parameters)
        {
            lock (this.locker)
            {
                if (this.writer == null)
                {
                    this.EnableLogging();
                }

                if (this.writer != null)
                {
                    if (parameters.Length == 0)
                    {
                        this.writer.WriteLine("{0:o}: {1}::: {2} --- {3}", dateTime, level, context, message);
                        this.writer.WriteLine("\t {0}", exception);
                    }
                    else
                    {
                        this.writer.WriteLine("{0:o}: {1}::: {2} --- {3}", dateTime, level, context, string.Format(message, parameters));
                        this.writer.WriteLine("\t {0}", exception);
                    }

                    this.writer.Flush();
                }
            }
        }

        private void EnableLogging()
        {
            var enableLoggingAsync = this.EnableLoggingAsync();
            TaskExtensions.WaitAllSafe(enableLoggingAsync);

            if (enableLoggingAsync.IsCompleted && !enableLoggingAsync.IsFaulted)
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
                await ApplicationContext.ApplicationLocalFolder.CreateFileAsync(
                        string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd-HH-mm-ss-ffff}.log", DateTime.Now),
                        CreationCollisionOption.OpenIfExists);

            var stream = await storageFile.OpenReadWriteAsync();

            return new StreamWriter(stream);
        }
    }
}
