// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    using Windows.Foundation;
    using Windows.Storage.Streams;

    public class MediaStreamDownloadService : IMediaStreamDownloadService
    {
        private const int DefaultBufferSize = 0x10000;

        private readonly ILogger logger;

        public MediaStreamDownloadService(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("MediaStreamDownloadService");
        }

        public async Task<INetworkRandomAccessStream> GetStreamAsync(string url)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Stream requested at url '{0}'.", url);
            }

            var webRequest = WebRequest.CreateHttp(url);
            webRequest.Method = "GET";
            webRequest.AllowReadStreamBuffering = false;
            var response = await webRequest.GetResponseAsync();

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Got response. ContentLength: {0}, ContentType: {1}.", response.ContentLength, response.ContentType);
            }

            long start = Math.Max(response.ContentLength - DefaultBufferSize, 0);

            HttpClient client = new HttpClient(new HttpClientHandler());
            client.DefaultRequestHeaders.Range = new RangeHeaderValue(start, response.ContentLength); 
            
            var readCount = (int)(response.ContentLength - start);

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Requestion end of stream. Start: {0}, End: {1}, Read Count: {2}.", start, response.ContentLength, readCount);
            }

            var audioStreamEnd = await client.GetStreamAsync(url);

            var data = new byte[response.ContentLength];
            var read = await audioStreamEnd.ReadAsync(data, (int)start, readCount);

            if (this.logger.IsDebugEnabled || this.logger.IsWarningEnabled)
            {
                if (read == readCount)
                {
                    this.logger.Debug("Got end of the stream. Expected read count from stream {0}.", read);
                }
                else
                {
                    this.logger.Warning("Got end of the stream. Unexpected read count from stream {0}.", read);
                }
            }

            return new MemoryRandomAccessStream(response.GetResponseStream(), data, response.ContentType, read);
        }

        private class MemoryRandomAccessStream : INetworkRandomAccessStream
        {
            private readonly object locker = new object();

            private readonly int endFilled;

            private readonly int contentLength;
            private byte[] data;

            private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            private Stream networkStream;
            private Task readTask;

            private ulong currentPosition = 0;
            private ulong readPosition = 0;

            private double latestDownloadProgressUpdate = 0;
            
            public MemoryRandomAccessStream(Stream networkStream, byte[] data, string contentType, int endFilled)
            {
                if (networkStream == null)
                {
                    throw new ArgumentNullException("networkStream");
                }

                this.ContentType = contentType;

                this.contentLength = data.Length;
                this.data = data;
                this.endFilled = endFilled;
                this.networkStream = networkStream;

                var cancellationToken = this.cancellationTokenSource.Token;

                this.readTask =
                    Task.Factory.StartNew(() => this.SafeDownloadSream(cancellationToken), cancellationToken)
                        .ContinueWith(
                            t =>
                                {
                                    this.DisposeNetworkDownloader();
                                    this.readTask = null;
                                });
            }

            ~MemoryRandomAccessStream()
            {
                this.Dispose(disposing: false);
            }

            public event EventHandler<double> DownloadProgressChanged;

            public string ContentType { get; private set; }

            public bool CanRead
            {
                get
                {
                    return true;
                }
            }

            public bool CanWrite
            {
                get
                {
                    return false;
                }
            }

            public ulong Position
            {
                get
                {
                    lock (this.locker)
                    {
                        return this.currentPosition;
                    }
                }
            }

            public ulong Size
            {
                get
                {
                    lock (this.locker)
                    {
                        return (ulong)this.contentLength;
                    }
                }

                set
                {
                    throw new NotSupportedException();
                }
            }

            public void Dispose()
            {
                this.Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            public void Seek(ulong position)
            {
                lock (this.locker)
                {
                    this.currentPosition = position;
                }
            }

            public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
            {
                return AsyncInfo.Run<IBuffer, uint>(async (token, progress) =>
                    {
                        progress.Report(0);

                        bool fReading = true;
                        do
                        {
                            lock (locker)
                            {
                                fReading = this.readPosition < (this.currentPosition + count)
                                    && this.currentPosition + (ulong)count < (ulong)this.contentLength;
                            }

                            if (fReading)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    return buffer;
                                }

                                await Task.Delay(10);
                            }
                        } 
                        while (fReading && this.readTask != null);

                        lock (locker)
                        {
                            int length = (int)Math.Min(count, this.contentLength - (int)this.currentPosition);
                            this.data.CopyTo((int)this.currentPosition, buffer, 0, length);
                            buffer.Length = (uint)length;
                        }

                        return buffer;
                    });
            }

            public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
            {
                throw new NotSupportedException();
            }

            public IAsyncOperation<bool> FlushAsync()
            {
                throw new NotSupportedException();
            }

            public IInputStream GetInputStreamAt(ulong position)
            {
                throw new NotSupportedException();
            }

            public IOutputStream GetOutputStreamAt(ulong position)
            {
                throw new NotSupportedException();
            }

            public IRandomAccessStream CloneStream()
            {
                throw new NotSupportedException();
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.DisposeNetworkDownloader();

                    lock (this.locker)
                    {
                        this.DownloadProgressChanged = null;
                        this.data = null;
                    }
                }
            }

            private void DisposeNetworkDownloader()
            {
                lock (this.locker)
                {
                    if (this.readTask != null && !this.cancellationTokenSource.IsCancellationRequested)
                    {
                        try
                        {
                            this.cancellationTokenSource.Cancel();
                            this.cancellationTokenSource.Dispose();
                        }
                        catch
                        {
                        }
                    }

                    this.readTask = null;
                    this.cancellationTokenSource = null;

                    if (this.networkStream != null)
                    {
                        this.networkStream.Dispose();
                        this.networkStream = null;
                    }
                }
            }

            private void RaiseDownloadProgressChanged(double downloadProgress)
            {
                if (Math.Abs(downloadProgress - 1.0) < 0.001 
                    || downloadProgress - this.latestDownloadProgressUpdate > 0.05)
                {
                    this.latestDownloadProgressUpdate = downloadProgress;
                    var handler = this.DownloadProgressChanged;
                    if (handler != null)
                    {
                        handler(this, downloadProgress);
                    }
                }
            }

            private void SafeDownloadSream(CancellationToken cancellationToken)
            {
                double downloadProgress = 0d;

                bool canRead;
                lock (this.locker)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    canRead = this.networkStream.CanRead;
                }

                while (canRead)
                {
                    int currentRead = DefaultBufferSize;

                    lock (this.locker)
                    {
                        if ((int)this.readPosition >= this.contentLength - this.endFilled)
                        {
                            break;
                        }

                        if (currentRead + (int)this.readPosition >= (this.contentLength - this.endFilled))
                        {
                            currentRead = (this.contentLength - this.endFilled) - (int)this.readPosition;
                        }
                    }

                    int read;
                    lock (this.locker)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        read = this.networkStream.Read(this.data, (int)this.readPosition, currentRead);
                    }

                    if (read == 0)
                    {
                        break;
                    }

                    lock (this.locker)
                    {
                        this.readPosition += (ulong)read;
                        downloadProgress = (double)this.readPosition / (double)this.contentLength;
                    }

                    this.RaiseDownloadProgressChanged(downloadProgress);

                    lock (this.locker)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        canRead = this.networkStream.CanRead;
                    }
                }

                downloadProgress = 1d;
                this.RaiseDownloadProgressChanged(downloadProgress);
            }
        }
    }
}