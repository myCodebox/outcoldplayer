// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;

    using Windows.Foundation;
    using Windows.Storage.Streams;

    public class MediaStreamDownloadService : IMediaStreamDownloadService
    {
        private const int DefaultBufferSize = 0x10000;

        private readonly ILogger logger;
        private readonly HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(20) };

        private volatile CancellationTokenSource cancellationTokenSource;

        public MediaStreamDownloadService(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("MediaStreamDownloadService");
        }

        public async Task<INetworkRandomAccessStream> GetStreamAsync(string url)
        {
            try
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Stream requested at url '{0}'.", url);
                }

                var source = this.cancellationTokenSource;
                if (source != null)
                {
                    this.client.CancelPendingRequests();
                    source.Cancel();
                }

                source = this.cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = source.Token;

                var response = await this.client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Got response. ContentLength: {0}, ContentType: {1}.", response.Content.Headers.ContentLength, response.Content.Headers.ContentType);
                }

                if (!response.Content.Headers.ContentLength.HasValue)
                {
                    this.logger.Error("Headers does not contains Content Length. Returning null.");
                    return null;
                }

                var contentLength = response.Content.Headers.ContentLength.Value;

                long start = Math.Max(response.Content.Headers.ContentLength.Value - DefaultBufferSize, 0);

                var readCount = (int)(response.Content.Headers.ContentLength - start);

                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Requestion end of stream. Start: {0}, End: {1}, Read Count: {2}.", start, response.Content.Headers.ContentLength, readCount);
                }

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Range = new RangeHeaderValue(start, contentLength);
                var streamResponse = await this.client.SendAsync(request, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                var data = new byte[contentLength];
                int read;

                using (var audioStreamEnd = await streamResponse.Content.ReadAsStreamAsync())
                {
                    read = await audioStreamEnd.ReadAsync(data, (int)start, readCount, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();

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

                this.cancellationTokenSource = null;

                return new MemoryRandomAccessStream(this.logger, await response.Content.ReadAsStreamAsync(), data, response.Content.Headers.ContentType.MediaType, read);
            }
            catch (Exception e)
            {
                this.logger.Error("Exception while loading stream");
                this.logger.LogErrorException(e);
                return null;
            }
        }

        private class MemoryRandomAccessStream : INetworkRandomAccessStream
        {
            private readonly ILogger logger;

            private readonly object locker = new object();

            private readonly int endFilled;
            private readonly int contentLength;

            private readonly byte[] readBuffer = new byte[DefaultBufferSize];
            private byte[] data;

            private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            private Stream networkStream;
            private Task readTask;

            private ulong currentPosition;
            private ulong readPosition;

            private double latestDownloadProgressUpdate;
            
            public MemoryRandomAccessStream(ILogger logger, Stream networkStream, byte[] data, string contentType, int endFilled)
            {
                if (networkStream == null)
                {
                    throw new ArgumentNullException("networkStream");
                }

                this.ContentType = contentType;

                this.contentLength = data.Length;
                this.logger = logger;
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
                    this.logger.Warning("set_Size is not supported.");
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

                        bool fReading;
                        do
                        {
                            lock (this.locker)
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

                        if (this.data != null)
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
                this.logger.Warning("WriteAsync is not supported.");
                throw new NotSupportedException();
            }

            public IAsyncOperation<bool> FlushAsync()
            {
                this.logger.Warning("FlushAsync is not supported.");
                throw new NotSupportedException();
            }

            public IInputStream GetInputStreamAt(ulong position)
            {
                this.logger.Warning("GetInputStreamAt ({0}) is not supported.", position);
                throw new NotSupportedException();
            }

            public IOutputStream GetOutputStreamAt(ulong position)
            {
                this.logger.Warning("GetOutputStreamAt ({0}) is not supported.", position);
                throw new NotSupportedException();
            }

            public IRandomAccessStream CloneStream()
            {
                this.logger.Warning("CloneStream is not supported.");
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
                try
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
                        cancellationToken.ThrowIfCancellationRequested();
                        read = this.networkStream.Read(this.readBuffer, 0, currentRead);

                        if (read == 0)
                        {
                            break;
                        }

                        if (this.data != null)
                        {
                            Array.Copy(this.readBuffer, 0, this.data, (int)this.readPosition, read);
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
                catch (TaskCanceledException exception)
                {
                    this.logger.Error("Downloading task was cancelled");
                    this.logger.LogDebugException(exception);
                }
                catch (Exception exception)
                {
                    this.logger.Error("Exception while reading stream");
                    this.logger.LogErrorException(exception);
                }
            }
        }
    }
}