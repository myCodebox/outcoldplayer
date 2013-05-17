// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;

    using Windows.Foundation;
    using Windows.Storage;
    using Windows.Storage.Streams;

    public class MediaStreamDownloadService : IMediaStreamDownloadService
    {
        private const int DefaultBufferSize = 0x10000;

        private const string RangeQueryParamName = "range=";

        private readonly ILogger logger;

        private readonly HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(20) };

        private volatile CancellationTokenSource cancellationTokenSource;

        public MediaStreamDownloadService(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("MediaStreamDownloadService");
        }

        public async Task<INetworkRandomAccessStream> GetStreamAsync(string url)
        {
            CancellationTokenSource source = null;

            try
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Stream requested at url '{0}'.", url);
                }

                var previousCancellationTokenSource = this.cancellationTokenSource;
                if (previousCancellationTokenSource != null)
                {
                    try
                    {
                        previousCancellationTokenSource.Cancel();
                    }
                    catch (ObjectDisposedException)
                    {
                    }

                    this.client.CancelPendingRequests();
                }

                source = this.cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = source.Token;

                var response =
                    await
                        this.client.SendAsync(
                            new HttpRequestMessage(HttpMethod.Get, url),
                            HttpCompletionOption.ResponseHeadersRead,
                            cancellationToken);

                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug(
                        "Got response. ContentLength: {0}, ContentType: {1}.",
                        response.Content.Headers.ContentLength,
                        response.Content.Headers.ContentType);
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
                    this.logger.Debug(
                        "Requestion end of stream. Start: {0}, End: {1}, Read Count: {2}.",
                        start,
                        response.Content.Headers.ContentLength,
                        readCount);
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

                return new MemoryRandomAccessStream(
                    this.logger,
                    await response.Content.ReadAsStreamAsync(),
                    data,
                    response.Content.Headers.ContentType.MediaType,
                    read);
            }
            catch (HttpRequestException exception)
            {
                if (exception.InnerException is IOException)
                {
                    this.logger.Debug(exception, "GetStreamAsync: HttpRequestException.");
                }
                else
                {
                    this.logger.Error(exception, "GetStreamAsync: Exception while loading stream.");
                }

                return null;
            }
            catch (IOException exception)
            {
                this.logger.Debug(exception, "GetStreamAsync: IOException.");
                return null;
            }
            catch (OperationCanceledException exception)
            {
                this.logger.Debug(exception, "GetStreamAsync: Operation was canceled.");
                return null;
            }
            catch (Exception e)
            {
                if (source == null || !source.IsCancellationRequested)
                {
                    this.logger.Error(e, "GetStreamAsync: Exception while loading stream");
                }

                return null;
            }
        }

        public async Task<INetworkRandomAccessStream> GetStreamAsync(string[] urls)
        {
            CancellationTokenSource source = null;

            try
            {
                if (urls == null || urls.Length == 0)
                {
                    return null;
                }

                long chunkStart;
                long contentLength;

                var lastUri = urls[urls.Length - 1];
                if (!GetChunkPosition(lastUri, out chunkStart, out contentLength))
                {
                    this.logger.Warning("Could not parse start and end range from url {0}", lastUri);
                    return null;
                }

                var previousCancellationTokenSource = this.cancellationTokenSource;
                if (previousCancellationTokenSource != null)
                {
                    try
                    {
                        previousCancellationTokenSource.Cancel();
                    }
                    catch (ObjectDisposedException)
                    {
                    }

                    this.client.CancelPendingRequests();
                }

                source = this.cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = source.Token;

                var data = new byte[contentLength];
                int read;

                var streamResponse = await this.client.GetAsync(lastUri, cancellationToken);
                using (var audioStreamEnd = await streamResponse.Content.ReadAsStreamAsync())
                {
                    read =
                        await
                            audioStreamEnd.ReadAsync(
                                data, (int)chunkStart, (int)(contentLength - chunkStart), cancellationToken);
                }

                return new MemoryRandomAccessStreamMultiStreams(
                    this.client,
                    source,
                    this.logger,
                    urls,
                    data,
                    streamResponse.Content.Headers.ContentType.MediaType,
                    read);

            }
            catch (HttpRequestException exception)
            {
                if (exception.InnerException is IOException)
                {
                    this.logger.Debug(exception, "GetStreamAsync (multiply urls): HttpRequestException.");
                }
                else
                {
                    this.logger.Error(exception, "GetStreamAsync (multiply urls): Exception while loading stream.");
                }

                return null;
            }
            catch (IOException exception)
            {
                this.logger.Debug(exception, "GetStreamAsync (multiply urls): IOException.");
                return null;
            }
            catch (OperationCanceledException exception)
            {
                this.logger.Debug(exception, "GetStreamAsync (multiply urls): Operation was canceled.");
                return null;
            }
            catch (Exception e)
            {
                if (source == null || !source.IsCancellationRequested)
                {
                    this.logger.Error(e, "GetStreamAsync (multiply urls): Exception while loading stream");
                }

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

            private readonly byte[] data;

            private readonly Task readTask;

            private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            private Stream networkStream;

            private ulong currentPosition;

            private ulong readPosition;

            private double latestDownloadProgressUpdate;

            public MemoryRandomAccessStream(
                ILogger logger, Stream networkStream, byte[] data, string contentType, int endFilled)
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
                    Task.Factory.StartNew(() => this.SafeDownloadStream(cancellationToken), cancellationToken)
                        .ContinueWith(t => this.DisposeNetworkDownloader());
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
                    lock (this.locker)
                    {
                        return this.currentPosition < (ulong)this.contentLength;
                    }
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

            public Task DownloadAsync()
            {
                return this.readTask;
            }

            public async Task SaveToFileAsync(IStorageFile file)
            {
                if (this.readPosition < (ulong)this.contentLength)
                {
                    throw new NotSupportedException("File is still in downloading state.");
                }

                await FileIO.WriteBytesAsync(file, this.data);
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

            public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(
                IBuffer buffer, uint count, InputStreamOptions options)
            {
                return AsyncInfo.Run<IBuffer, uint>(
                    async (token, progress) =>
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

                                await Task.Delay(10, token);
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
                    }
                }
            }

            private void DisposeNetworkDownloader()
            {
                lock (this.locker)
                {
                    if (this.readTask != null && this.cancellationTokenSource != null
                        && !this.cancellationTokenSource.IsCancellationRequested)
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

            private void SafeDownloadStream(CancellationToken cancellationToken)
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

                    this.readPosition = (ulong)this.contentLength;
                    downloadProgress = 1d;
                    this.RaiseDownloadProgressChanged(downloadProgress);
                }
                catch (HttpRequestException exception)
                {
                    if (exception.InnerException is IOException)
                    {
                        this.logger.Debug(exception, "SafeDownloadStream: HttpRequestException.");
                    }
                    else
                    {
                        this.logger.Error(exception, "SafeDownloadStream: Exception while reading stream.");
                    }
                }
                catch (IOException exception)
                {
                    this.logger.Debug(exception, "SafeDownloadStream: IOException.");
                }
                catch (OperationCanceledException exception)
                {
                    this.logger.Debug(exception, "SafeDownloadStream: Downloading task was canceled.");
                }
                catch (Exception exception)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        this.logger.Debug(exception, "SafeDownloadStream: Downloading task was canceled .");
                    }
                    else
                    {
                        this.logger.Error(exception, "SafeDownloadStream: Exception while reading stream.");
                    }
                }
            }
        }

        private class MemoryRandomAccessStreamMultiStreams : INetworkRandomAccessStream
        {
            private readonly object locker = new object();

            private readonly Task downloadTask;

            private readonly ILogger logger;

            private readonly string[] urls;

            private readonly byte[] data;

            private readonly string mediaType;

            private readonly int lastChunk;

            private ulong readPosition;

            private ulong currentPosition;

            private CancellationTokenSource cancellationTokenSource;

            public MemoryRandomAccessStreamMultiStreams(
                HttpClient httpClient,
                CancellationTokenSource cancellationTokenSource,
                ILogger logger,
                string[] urls,
                byte[] data,
                string mediaType,
                int lastChunk)
            {
                this.cancellationTokenSource = cancellationTokenSource;
                this.logger = logger;
                this.urls = urls;
                this.data = data;
                this.mediaType = mediaType;
                this.lastChunk = lastChunk;

                this.downloadTask = this.DownloadStream(httpClient, cancellationTokenSource.Token);
            }

            public void Dispose()
            {
                lock (this.locker)
                {
                    if (this.cancellationTokenSource != null && !this.cancellationTokenSource.IsCancellationRequested)
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

                    this.cancellationTokenSource = null;
                }
            }

            public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(
                IBuffer buffer, uint count, InputStreamOptions options)
            {
                return AsyncInfo.Run<IBuffer, uint>(
                    async (token, progress) =>
                    {
                        progress.Report(0);

                        bool fReading;
                        do
                        {
                            lock (this.locker)
                            {
                                fReading = this.readPosition < (this.currentPosition + count)
                                    && this.currentPosition < (ulong)this.lastChunk;
                            }

                            if (fReading)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    return buffer;
                                }

                                await Task.Delay(10, token);
                            }
                        }
                        while (fReading && this.downloadTask != null);

                        if (this.data != null)
                        {
                            int length = (int)Math.Min(count, this.data.Length - (int)this.currentPosition);
                            this.data.CopyTo((int)this.currentPosition, buffer, 0, length);
                            buffer.Length = (uint)length;
                        }

                        return buffer;
                    });
            }

            public void Seek(ulong position)
            {
                lock (this.locker)
                {
                    this.currentPosition = position;
                }
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

            public bool CanRead
            {
                get
                {
                    lock (this.locker)
                    {
                        return this.currentPosition < (ulong)this.data.Length;
                    }
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
                    return (ulong)this.data.Length;
                }

                set
                {
                    this.logger.Warning("set_Size is not supported.");
                    throw new NotSupportedException();
                }
            }

            public string ContentType
            {
                get
                {
                    return this.mediaType;
                }
            }

            private void RaiseDownloadProgressChanged(double downloadProgress)
            {
                var handler = this.DownloadProgressChanged;
                if (handler != null)
                {
                    handler(this, downloadProgress);
                }
            }

            public event EventHandler<double> DownloadProgressChanged;

            public Task DownloadAsync()
            {
                return this.downloadTask;
            }

            public async Task SaveToFileAsync(IStorageFile file)
            {
                lock (this.locker)
                {
                    if (this.readPosition < (ulong)data.Length)
                    {
                        throw new NotSupportedException("File is still in downloading state.");
                    }
                }

                await FileIO.WriteBytesAsync(file, this.data);
            }

            private async Task DownloadStream(HttpClient client, CancellationToken token)
            {
                try
                {
                    for (int i = 0; i < this.urls.Length - 1; i++)
                    {
                        var uri = this.urls[i];

                        long start;
                        long end;
                        if (GetChunkPosition(uri, out start, out end))
                        {
                            lock (this.locker)
                            {
                                this.readPosition = (ulong)Math.Max(start - 1, 0);
                            }

                            double downloadProgress;
                            lock (this.locker)
                            {
                                downloadProgress = (double)this.readPosition / (double)this.data.Length;
                            }

                            this.RaiseDownloadProgressChanged(downloadProgress);

                            var response = await client.GetAsync(uri, token);
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                int read = await stream.ReadAsync(this.data, (int)start, (int)end - (int)start, token);

                                if (read != (int)end - (int)start)
                                {
                                    this.logger.Warning(
                                        "We read not the same value as we expected from url {0}. We read {1}.",
                                        uri,
                                        read);
                                }
                            }
                        }
                        else
                        {
                            this.logger.Warning("Could not parse start and end range from url {0}", uri);
                        }
                    }

                    this.readPosition = (ulong)this.data.Length;

                    this.RaiseDownloadProgressChanged(1.0d);
                }
                catch (HttpRequestException exception)
                {
                    if (exception.InnerException is IOException)
                    {
                        this.logger.Debug(
                            exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: HttpRequestException.");
                    }
                    else
                    {
                        this.logger.Error(
                            exception,
                            "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: Exception while reading stream.");
                    }
                }
                catch (IOException exception)
                {
                    this.logger.Debug(
                        exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: IOException.");
                }
                catch (OperationCanceledException exception)
                {
                    this.logger.Debug(
                        exception,
                        "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: Downloading task was canceled.");
                }
                catch (Exception exception)
                {
                    if (token.IsCancellationRequested)
                    {
                        this.logger.Debug(
                            exception,
                            "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: Downloading task was canceled .");
                    }
                    else
                    {
                        this.logger.Error(
                            exception,
                            "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: Exception while reading stream.");
                    }
                }
            }
        }

        private static bool GetChunkPosition(string url, out long start, out long end)
        {
            start = 0;
            end = 0;

            var lastUri = new Uri(url);
            var rangeParam =
                lastUri.Query.Split('&')
                    .FirstOrDefault(x => x.StartsWith(RangeQueryParamName, StringComparison.OrdinalIgnoreCase));
            if (rangeParam == null)
            {
                return false;
            }

            var rangeEnd = rangeParam.Substring(RangeQueryParamName.Length).Split('-');
            if (rangeEnd.Length != 2)
            {
                return false;
            }

            start = long.Parse(rangeEnd[0]);
            end = long.Parse(rangeEnd[1]);

            return true;
        }
    }
}