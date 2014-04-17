<<<<<<< HEAD
﻿// --------------------------------------------------------------------------------------------------------------------
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

    using Windows.Foundation;
    using Windows.Storage;
    using Windows.Storage.Streams;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    public class MediaStreamDownloadService : IMediaStreamDownloadService
    {
        private const int DefaultBufferSize = 0x10000;

        private const string RangeQueryParamName = "range=";

        private readonly ILogger logger;
        private readonly HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(60) };

        public MediaStreamDownloadService(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("MediaStreamDownloadService");
        }

        public async Task<INetworkRandomAccessStream> GetStreamAsync(string url, CancellationToken token)
        {
            try
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Stream requested at url '{0}'.", url);
                }

                var response =
                    await
                        this.client.SendAsync(
                            new HttpRequestMessage(HttpMethod.Get, url),
                            HttpCompletionOption.ResponseHeadersRead,
                            token);

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
                var streamResponse = await this.client.SendAsync(request, token);

                token.ThrowIfCancellationRequested();

                var data = new byte[contentLength];
                int read;

                using (var audioStreamEnd = await streamResponse.Content.ReadAsStreamAsync())
                {
                    read = await audioStreamEnd.ReadAsync(data, (int)start, readCount, token);
                }

                token.ThrowIfCancellationRequested();

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

                return new MemoryRandomAccessStream(
                    this.logger,
                    await response.Content.ReadAsStreamAsync(),
                    data,
                    response.Content.Headers.ContentType.MediaType,
                    read,
                    token);
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
                if (!token.IsCancellationRequested)
                {
                    this.logger.Error(e, "GetStreamAsync: Exception while loading stream");
                }

                return null;
            }
        }

        public async Task<INetworkRandomAccessStream> GetStreamAsync(string[] urls, CancellationToken token)
        {
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

                var data = new byte[contentLength];
                int read;

                var streamResponse = await this.client.GetAsync(lastUri, token);
                using (var audioStreamEnd = await streamResponse.Content.ReadAsStreamAsync())
                {
                    read = await audioStreamEnd.ReadAsync(data, (int)chunkStart, (int)(contentLength - chunkStart), token);
                }

                return new MemoryRandomAccessStreamMultiStreams(
                    this.client,
                    this.logger,
                    urls,
                    data,
                    streamResponse.Content.Headers.ContentType.MediaType,
                    (int)chunkStart,
                    token);

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
                if (!token.IsCancellationRequested)
                {
                    this.logger.Error(e, "GetStreamAsync (multiply urls): Exception while loading stream");
                }

                return null;
            }
        }

        public async Task<IRandomAccessStream> GetCachedStreamAsync(IStorageFile storageFile, CancellationToken token)
        {
            InMemoryRandomAccessStream memoryRandomAccessStream = new InMemoryRandomAccessStream();

            using (DataWriter writer = new DataWriter(memoryRandomAccessStream))
            {
                using (var stream = await storageFile.OpenStreamForReadAsync())
                {
                    byte[] buffer = new byte[DefaultBufferSize];

                    var chunks = stream.Length / DefaultBufferSize;

                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }

                    for (int i = 1; i <= chunks; i++)
                    {
                        stream.Seek(stream.Length - (i * DefaultBufferSize), SeekOrigin.Begin);
                        await stream.ReadAsync(buffer, 0, DefaultBufferSize, token);
                        writer.WriteBytes(buffer);
                    }

                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }

                    long lastChunkLength = stream.Length % DefaultBufferSize;
                    if (lastChunkLength > 0)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        byte[] lastChunk = new byte[lastChunkLength];
                        await stream.ReadAsync(lastChunk, 0, lastChunk.Length, token);
                        writer.WriteBytes(lastChunk);
                    }
                }

                await writer.StoreAsync();
                await writer.FlushAsync();
                writer.DetachStream();
            }

            return memoryRandomAccessStream;
        }
        
        private static bool GetChunkPosition(string url, out long start, out long end)
        {
            start = 0;
            end = 0;

            var lastUri = new Uri(url);
            var rangeParam = lastUri.Query.Split('&').FirstOrDefault(x => x.StartsWith(RangeQueryParamName, StringComparison.OrdinalIgnoreCase));
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

        private static async Task WriteSongToCache(IStorageFile file, byte[] data)
        {
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                var chunks = data.Length / DefaultBufferSize;

                if (data.Length % DefaultBufferSize > 0)
                {
                    await stream.WriteAsync(data, chunks * DefaultBufferSize, data.Length - (chunks * DefaultBufferSize));
                }

                for (int i = chunks - 1; i >= 0; i--)
                {
                    await stream.WriteAsync(data, i * DefaultBufferSize, DefaultBufferSize);
                }
            }
        }

        private class MemoryRandomAccessStream : INetworkRandomAccessStream
        {
            private readonly ILogger logger;

            private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

            private readonly int endFilled;

            private readonly CancellationToken token;

            private readonly int contentLength;

            private readonly byte[] readBuffer = new byte[DefaultBufferSize];
            private readonly byte[] data;

            private readonly Task readTask;

            private Stream networkStream;

            private ulong currentPosition;
            private ulong readPosition;

            private double latestDownloadProgressUpdate;

            private bool isFailed;

            public MemoryRandomAccessStream(ILogger logger, Stream networkStream, byte[] data, string contentType, int endFilled, CancellationToken token)
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
                this.token = token;
                this.networkStream = networkStream;

                this.readTask = this.SafeDownloadStream().ContinueWith(t => this.DisposeNetworkDownloader());
            }

            ~MemoryRandomAccessStream()
            {
                this.Dispose(disposing: false);
            }

            public event EventHandler<double> DownloadProgressChanged;

            public bool IsReady { get; private set; }

            public bool IsFailed
            {
                get
                {
                    return this.isFailed || this.token.IsCancellationRequested;
                }

                private set
                {
                    this.isFailed = value;
                }
            }

            public string ContentType { get; private set; }

            public bool CanRead
            {
                get
                {
                    try
                    {
                        this.semaphore.Wait(this.token);

                        try
                        {
                            return this.currentPosition < (ulong)this.contentLength;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return false;
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
                    try
                    {
                        this.semaphore.Wait(this.token);

                        try
                        {
                            return this.currentPosition;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return this.currentPosition;
                    }
                }
            }

            public ulong Size
            {
                get
                {
                    try
                    {
                        this.semaphore.Wait(this.token);

                        try
                        {
                            return (ulong)this.contentLength;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }
                    }
                    catch (OperationCanceledException)
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

                await WriteSongToCache(file, this.data);
            }

            public void Dispose()
            {
                this.Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            public void Seek(ulong position)
            {
                try
                {
                    this.semaphore.Wait(this.token);

                    try
                    {
                        this.currentPosition = position;
                    }
                    finally
                    {
                        this.semaphore.Release(1);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }

            public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
            {
                return AsyncInfo.Run<IBuffer, uint>(async (t, progress) =>
                    {
                        progress.Report(0);

                        bool fReading;
                        do
                        {
                            await this.semaphore.WaitAsync(t).ConfigureAwait(continueOnCapturedContext: false);

                            try
                            {
                                fReading = this.readPosition < (this.currentPosition + count)
                                    && this.currentPosition + (ulong)count < (ulong)this.contentLength;
                            }
                            finally
                            {
                                this.semaphore.Release(1);
                            }

                            if (fReading)
                            {
                                if (t.IsCancellationRequested)
                                {
                                    return buffer;
                                }

                                await Task.Delay(10, t);
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
                    this.DownloadProgressChanged = null;
                }
            }

            private void DisposeNetworkDownloader()
            {
                try
                {
                    if (this.networkStream != null)
                    {
                        this.networkStream.Dispose();
                        this.networkStream = null;
                    }
                }
                catch
                {
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

            private async Task SafeDownloadStream()
            {
                await Task.Yield();

                try
                {
                    double downloadProgress = 0d;

                    bool canRead;

                    await this.semaphore.WaitAsync(this.token).ConfigureAwait(continueOnCapturedContext: false);

                    if (this.token.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        canRead = this.networkStream.CanRead;
                    }
                    finally
                    {
                        this.semaphore.Release(1);
                    }

                    while (canRead)
                    {
                        int currentRead = DefaultBufferSize;

                        await this.semaphore.WaitAsync(this.token).ConfigureAwait(continueOnCapturedContext: false);

                        if (this.token.IsCancellationRequested)
                        {
                            return;
                        }

                        try
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
                        finally
                        {
                            this.semaphore.Release(1);
                        }

                        int read;

                        if (this.token.IsCancellationRequested)
                        {
                            return;
                        }

                        read = await this.networkStream.ReadAsync(this.readBuffer, 0, currentRead, this.token);

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

                        await this.semaphore.WaitAsync(this.token).ConfigureAwait(continueOnCapturedContext: false);

                        if (this.token.IsCancellationRequested)
                        {
                            return;
                        }

                        try
                        {
                            canRead = this.networkStream.CanRead;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }
                    }

                    this.readPosition = (ulong)this.contentLength;
                    this.IsReady = true;

                    downloadProgress = 1d;
                    this.RaiseDownloadProgressChanged(downloadProgress);
                }
                catch (HttpRequestException exception)
                {
                    this.IsFailed = true;

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
                    this.IsFailed = true;

                    this.logger.Debug(exception, "SafeDownloadStream: IOException.");
                }
                catch (OperationCanceledException exception)
                {
                    this.IsFailed = true;

                    this.logger.Debug(exception, "SafeDownloadStream: Downloading task was canceled.");
                    throw;
                }
                catch (Exception exception)
                {
                    this.IsFailed = true;

                    if (this.token.IsCancellationRequested)
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
            private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
            private readonly Task downloadTask;
            
            private readonly ILogger logger;
            private readonly string[] urls;
            private readonly byte[] data;
            private readonly string mediaType;
            private readonly int lastChunk;
            private ulong readPosition;
            private ulong currentPosition;

            private CancellationToken token;

            private bool isFailed;

            public MemoryRandomAccessStreamMultiStreams(
                HttpClient httpClient,
                ILogger logger, 
                string[] urls, 
                byte[] data, 
                string mediaType, 
                int lastChunk,
                CancellationToken token)
            {
                this.token = token;
                this.logger = logger;
                this.urls = urls;
                this.data = data;
                this.mediaType = mediaType;
                this.lastChunk = lastChunk;

                this.downloadTask = this.DownloadStream(httpClient);
            }

            ~MemoryRandomAccessStreamMultiStreams()
            {
                this.Dispose(disposing: false);
            }

            public event EventHandler<double> DownloadProgressChanged;

            public bool CanRead
            {
                get
                {
                    try
                    {
                        this.semaphore.Wait(this.token);

                        try
                        {
                            return this.currentPosition < (ulong)this.data.Length;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }
                    }
                    catch(OperationCanceledException)
                    {
                        return false;
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
                    try
                    {
                        this.semaphore.Wait(this.token);

                        try
                        {
                            return this.currentPosition;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }
                    }
                    catch (OperationCanceledException)
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

            public bool IsReady { get; private set; }

            public bool IsFailed
            {
                get
                {
                    return this.isFailed || this.token.IsCancellationRequested;
                }

                private set
                {
                    this.isFailed = value;
                }
            }

            public void Dispose()
            {
                this.Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(
                IBuffer buffer, uint count, InputStreamOptions options)
            {
                return AsyncInfo.Run<IBuffer, uint>(async (t, progress) =>
                {
                    progress.Report(0);

                    bool fReading;
                    do
                    {
                        await this.semaphore.WaitAsync(t).ConfigureAwait(continueOnCapturedContext: false);

                        try
                        {
                            fReading = this.readPosition < (this.currentPosition + count)
                                && this.currentPosition < (ulong)this.lastChunk;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }

                        if (fReading)
                        {
                            if (t.IsCancellationRequested)
                            {
                                return buffer;
                            }

                            await Task.Delay(10, t);
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
                try
                {
                    this.semaphore.Wait(this.token);

                    try
                    {
                        this.currentPosition = position;
                    }
                    finally
                    {
                        this.semaphore.Release(1);
                    }
                }
                catch (OperationCanceledException)
                {
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

            public Task DownloadAsync()
            {
                return this.downloadTask;
            }

            public async Task SaveToFileAsync(IStorageFile file)
            {
                await this.semaphore.WaitAsync(this.token).ConfigureAwait(continueOnCapturedContext: false);

                try
                {
                    if (this.readPosition < (ulong)this.data.Length)
                    {
                        throw new NotSupportedException("File is still in downloading state.");
                    }
                }
                finally
                {
                    this.semaphore.Release(1);
                }

                await WriteSongToCache(file, this.data);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.DownloadProgressChanged = null;
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

            private async Task DownloadStream(HttpClient client)
            {
                await Task.Yield();

                try
                {
                    for (int i = 0; i < this.urls.Length - 1; i++)
                    {
                        var uri = this.urls[i];

                        token.ThrowIfCancellationRequested();

                        long start;
                        long end;
                        if (GetChunkPosition(uri, out start, out end))
                        {
                            await this.semaphore.WaitAsync(this.token).ConfigureAwait(continueOnCapturedContext: false);

                            try
                            {
                                this.readPosition = (ulong)Math.Max(start - 1, 0);
                            }
                            finally
                            {
                                this.semaphore.Release(1);
                            }

                            double downloadProgress;

                            await this.semaphore.WaitAsync(this.token).ConfigureAwait(continueOnCapturedContext: false);

                            try
                            {
                                downloadProgress = (double)this.readPosition / (double)this.data.Length;
                            }
                            finally
                            {
                                this.semaphore.Release(1);
                            }

                            this.RaiseDownloadProgressChanged(downloadProgress);

                            var response = await client.GetAsync(uri, this.token);
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                int read = await stream.ReadAsync(this.data, (int)start, (int)end - (int)start, this.token);

                                if (read != (int)end - (int)start)
                                {
                                    this.logger.Warning("We read not the same value as we expected from url {0}. We read {1}.", uri, read);
                                }
                            }
                        }
                        else
                        {
                            this.logger.Warning("Could not parse start and end range from url {0}", uri);
                        }
                    }

                    this.readPosition = (ulong)this.data.Length;
                    this.IsReady = true;

                    this.RaiseDownloadProgressChanged(1.0d);
                }
                catch (HttpRequestException exception)
                {
                    this.IsFailed = true;

                    if (exception.InnerException is IOException)
                    {
                        this.logger.Debug(exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: HttpRequestException.");
                    }
                    else
                    {
                        this.logger.Error(exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: Exception while reading stream.");
                    }
                }
                catch (IOException exception)
                {
                    this.IsFailed = true;

                    this.logger.Debug(exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: IOException.");
                }
                catch (OperationCanceledException exception)
                {
                    this.IsFailed = true;

                    this.logger.Debug(exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: Downloading task was canceled.");
                    throw;
                }
                catch (Exception exception)
                {
                    this.IsFailed = true;

                    if (this.token.IsCancellationRequested)
                    {
                        this.logger.Debug(exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: Downloading task was canceled .");
                    }
                    else
                    {
                        this.logger.Error(exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: Exception while reading stream.");
                    }
                }
            }
        }
    }
=======
﻿// --------------------------------------------------------------------------------------------------------------------
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

    using Windows.Foundation;
    using Windows.Storage;
    using Windows.Storage.Streams;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    public class MediaStreamDownloadService : IMediaStreamDownloadService
    {
        private const int DefaultBufferSize = 0x10000;

        private const string RangeQueryParamName = "range=";

        private readonly ILogger logger;
        private readonly HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(60) };

        public MediaStreamDownloadService(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("MediaStreamDownloadService");
        }

        public async Task<INetworkRandomAccessStream> GetStreamAsync(string url, CancellationToken token)
        {
            try
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Stream requested at url '{0}'.", url);
                }

                var response =
                    await
                        this.client.SendAsync(
                            new HttpRequestMessage(HttpMethod.Get, url),
                            HttpCompletionOption.ResponseHeadersRead,
                            token);

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
                var streamResponse = await this.client.SendAsync(request, token);

                token.ThrowIfCancellationRequested();

                var data = new byte[contentLength];
                int read;

                using (var audioStreamEnd = await streamResponse.Content.ReadAsStreamAsync())
                {
                    read = await audioStreamEnd.ReadAsync(data, (int)start, readCount, token);
                }

                token.ThrowIfCancellationRequested();

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

                return new MemoryRandomAccessStream(
                    this.logger,
                    await response.Content.ReadAsStreamAsync(),
                    data,
                    response.Content.Headers.ContentType.MediaType,
                    read,
                    token);
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
                if (!token.IsCancellationRequested)
                {
                    this.logger.Error(e, "GetStreamAsync: Exception while loading stream");
                }

                return null;
            }
        }

        public async Task<INetworkRandomAccessStream> GetStreamAsync(string[] urls, CancellationToken token)
        {
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

                var data = new byte[contentLength];
                int read;

                var streamResponse = await this.client.GetAsync(lastUri, token);
                using (var audioStreamEnd = await streamResponse.Content.ReadAsStreamAsync())
                {
                    read = await audioStreamEnd.ReadAsync(data, (int)chunkStart, (int)(contentLength - chunkStart), token);
                }

                return new MemoryRandomAccessStreamMultiStreams(
                    this.client,
                    this.logger,
                    urls,
                    data,
                    streamResponse.Content.Headers.ContentType.MediaType,
                    read,
                    token);

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
                if (!token.IsCancellationRequested)
                {
                    this.logger.Error(e, "GetStreamAsync (multiply urls): Exception while loading stream");
                }

                return null;
            }
        }

        public async Task<IRandomAccessStream> GetCachedStreamAsync(IStorageFile storageFile, CancellationToken token)
        {
            InMemoryRandomAccessStream memoryRandomAccessStream = new InMemoryRandomAccessStream();

            using (DataWriter writer = new DataWriter(memoryRandomAccessStream))
            {
                using (var stream = await storageFile.OpenStreamForReadAsync())
                {
                    byte[] buffer = new byte[DefaultBufferSize];

                    var chunks = stream.Length / DefaultBufferSize;

                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }

                    for (int i = 1; i <= chunks; i++)
                    {
                        stream.Seek(stream.Length - (i * DefaultBufferSize), SeekOrigin.Begin);
                        await stream.ReadAsync(buffer, 0, DefaultBufferSize, token);
                        writer.WriteBytes(buffer);
                    }

                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }

                    long lastChunkLength = stream.Length % DefaultBufferSize;
                    if (lastChunkLength > 0)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        byte[] lastChunk = new byte[lastChunkLength];
                        await stream.ReadAsync(lastChunk, 0, lastChunk.Length, token);
                        writer.WriteBytes(lastChunk);
                    }
                }

                await writer.StoreAsync();
                await writer.FlushAsync();
                writer.DetachStream();
            }

            return memoryRandomAccessStream;
        }
        
        private static bool GetChunkPosition(string url, out long start, out long end)
        {
            start = 0;
            end = 0;

            var lastUri = new Uri(url);
            var rangeParam = lastUri.Query.Split('&').FirstOrDefault(x => x.StartsWith(RangeQueryParamName, StringComparison.OrdinalIgnoreCase));
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

        private static async Task WriteSongToCache(IStorageFile file, byte[] data)
        {
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                var chunks = data.Length / DefaultBufferSize;

                if (data.Length % DefaultBufferSize > 0)
                {
                    await stream.WriteAsync(data, chunks * DefaultBufferSize, data.Length - (chunks * DefaultBufferSize));
                }

                for (int i = chunks - 1; i >= 0; i--)
                {
                    await stream.WriteAsync(data, i * DefaultBufferSize, DefaultBufferSize);
                }
            }
        }

        private class MemoryRandomAccessStream : INetworkRandomAccessStream
        {
            private readonly ILogger logger;

            private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

            private readonly int endFilled;

            private readonly CancellationToken token;

            private readonly int contentLength;

            private readonly byte[] readBuffer = new byte[DefaultBufferSize];
            private readonly byte[] data;

            private readonly Task readTask;

            private Stream networkStream;

            private ulong currentPosition;
            private ulong readPosition;

            private double latestDownloadProgressUpdate;

            private bool isFailed;

            public MemoryRandomAccessStream(ILogger logger, Stream networkStream, byte[] data, string contentType, int endFilled, CancellationToken token)
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
                this.token = token;
                this.networkStream = networkStream;

                this.readTask = this.SafeDownloadStream().ContinueWith(t => this.DisposeNetworkDownloader());
            }

            ~MemoryRandomAccessStream()
            {
                this.Dispose(disposing: false);
            }

            public event EventHandler<double> DownloadProgressChanged;

            public bool IsReady { get; private set; }

            public bool IsFailed
            {
                get
                {
                    return this.isFailed || this.token.IsCancellationRequested;
                }

                private set
                {
                    this.isFailed = value;
                }
            }

            public string ContentType { get; private set; }

            public bool CanRead
            {
                get
                {
                    try
                    {
                        this.semaphore.Wait(this.token);

                        try
                        {
                            return this.currentPosition < (ulong)this.contentLength;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return false;
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
                    try
                    {
                        this.semaphore.Wait(this.token);

                        try
                        {
                            return this.currentPosition;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return this.currentPosition;
                    }
                }
            }

            public ulong Size
            {
                get
                {
                    try
                    {
                        this.semaphore.Wait(this.token);

                        try
                        {
                            return (ulong)this.contentLength;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }
                    }
                    catch (OperationCanceledException)
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

                await WriteSongToCache(file, this.data);
            }

            public void Dispose()
            {
                this.Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            public void Seek(ulong position)
            {
                try
                {
                    this.semaphore.Wait(this.token);

                    try
                    {
                        this.currentPosition = position;
                    }
                    finally
                    {
                        this.semaphore.Release(1);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }

            public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
            {
                return AsyncInfo.Run<IBuffer, uint>(async (t, progress) =>
                    {
                        progress.Report(0);

                        bool fReading;
                        do
                        {
                            await this.semaphore.WaitAsync(t).ConfigureAwait(continueOnCapturedContext: false);

                            try
                            {
                                fReading = this.readPosition < (this.currentPosition + count)
                                    && this.currentPosition + (ulong)count < (ulong)this.contentLength;
                            }
                            finally
                            {
                                this.semaphore.Release(1);
                            }

                            if (fReading)
                            {
                                if (t.IsCancellationRequested)
                                {
                                    return buffer;
                                }

                                await Task.Delay(10, t);
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
                    this.DownloadProgressChanged = null;
                }
            }

            private void DisposeNetworkDownloader()
            {
                try
                {
                    if (this.networkStream != null)
                    {
                        this.networkStream.Dispose();
                        this.networkStream = null;
                    }
                }
                catch
                {
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

            private async Task SafeDownloadStream()
            {
                await Task.Yield();

                try
                {
                    double downloadProgress = 0d;

                    bool canRead;

                    await this.semaphore.WaitAsync(this.token).ConfigureAwait(continueOnCapturedContext: false);

                    if (this.token.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        canRead = this.networkStream.CanRead;
                    }
                    finally
                    {
                        this.semaphore.Release(1);
                    }

                    while (canRead)
                    {
                        int currentRead = DefaultBufferSize;

                        await this.semaphore.WaitAsync(this.token).ConfigureAwait(continueOnCapturedContext: false);

                        if (this.token.IsCancellationRequested)
                        {
                            return;
                        }

                        try
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
                        finally
                        {
                            this.semaphore.Release(1);
                        }

                        int read;

                        if (this.token.IsCancellationRequested)
                        {
                            return;
                        }

                        read = await this.networkStream.ReadAsync(this.readBuffer, 0, currentRead, this.token);

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

                        await this.semaphore.WaitAsync(this.token).ConfigureAwait(continueOnCapturedContext: false);

                        if (this.token.IsCancellationRequested)
                        {
                            return;
                        }

                        try
                        {
                            canRead = this.networkStream.CanRead;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }
                    }

                    this.readPosition = (ulong)this.contentLength;
                    this.IsReady = true;

                    downloadProgress = 1d;
                    this.RaiseDownloadProgressChanged(downloadProgress);
                }
                catch (HttpRequestException exception)
                {
                    this.IsFailed = true;

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
                    this.IsFailed = true;

                    this.logger.Debug(exception, "SafeDownloadStream: IOException.");
                }
                catch (OperationCanceledException exception)
                {
                    this.IsFailed = true;

                    this.logger.Debug(exception, "SafeDownloadStream: Downloading task was canceled.");
                    throw;
                }
                catch (Exception exception)
                {
                    this.IsFailed = true;

                    if (this.token.IsCancellationRequested)
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
            private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
            private readonly Task downloadTask;
            
            private readonly ILogger logger;
            private readonly string[] urls;
            private readonly byte[] data;
            private readonly string mediaType;
            private readonly int lastChunk;
            private ulong readPosition;
            private ulong currentPosition;

            private CancellationToken token;

            private bool isFailed;

            public MemoryRandomAccessStreamMultiStreams(
                HttpClient httpClient,
                ILogger logger, 
                string[] urls, 
                byte[] data, 
                string mediaType, 
                int lastChunk,
                CancellationToken token)
            {
                this.token = token;
                this.logger = logger;
                this.urls = urls;
                this.data = data;
                this.mediaType = mediaType;
                this.lastChunk = lastChunk;

                this.downloadTask = this.DownloadStream(httpClient);
            }

            ~MemoryRandomAccessStreamMultiStreams()
            {
                this.Dispose(disposing: false);
            }

            public event EventHandler<double> DownloadProgressChanged;

            public bool CanRead
            {
                get
                {
                    try
                    {
                        this.semaphore.Wait(this.token);

                        try
                        {
                            return this.currentPosition < (ulong)this.data.Length;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }
                    }
                    catch(OperationCanceledException)
                    {
                        return false;
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
                    try
                    {
                        this.semaphore.Wait(this.token);

                        try
                        {
                            return this.currentPosition;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }
                    }
                    catch (OperationCanceledException)
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

            public bool IsReady { get; private set; }

            public bool IsFailed
            {
                get
                {
                    return this.isFailed || this.token.IsCancellationRequested;
                }

                private set
                {
                    this.isFailed = value;
                }
            }

            public void Dispose()
            {
                this.Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(
                IBuffer buffer, uint count, InputStreamOptions options)
            {
                return AsyncInfo.Run<IBuffer, uint>(async (t, progress) =>
                {
                    progress.Report(0);

                    bool fReading;
                    do
                    {
                        await this.semaphore.WaitAsync(t).ConfigureAwait(continueOnCapturedContext: false);

                        try
                        {
                            fReading = this.readPosition < (this.currentPosition + count)
                                && this.currentPosition < (ulong)this.lastChunk;
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }

                        if (fReading)
                        {
                            if (t.IsCancellationRequested)
                            {
                                return buffer;
                            }

                            await Task.Delay(10, t);
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
                try
                {
                    this.semaphore.Wait(this.token);

                    try
                    {
                        this.currentPosition = position;
                    }
                    finally
                    {
                        this.semaphore.Release(1);
                    }
                }
                catch (OperationCanceledException)
                {
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

            public Task DownloadAsync()
            {
                return this.downloadTask;
            }

            public async Task SaveToFileAsync(IStorageFile file)
            {
                await this.semaphore.WaitAsync(this.token).ConfigureAwait(continueOnCapturedContext: false);

                try
                {
                    if (this.readPosition < (ulong)this.data.Length)
                    {
                        throw new NotSupportedException("File is still in downloading state.");
                    }
                }
                finally
                {
                    this.semaphore.Release(1);
                }

                await WriteSongToCache(file, this.data);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.DownloadProgressChanged = null;
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

            private async Task DownloadStream(HttpClient client)
            {
                await Task.Yield();

                try
                {
                    for (int i = 0; i < this.urls.Length - 1; i++)
                    {
                        var uri = this.urls[i];

                        token.ThrowIfCancellationRequested();

                        long start;
                        long end;
                        if (GetChunkPosition(uri, out start, out end))
                        {
                            await this.semaphore.WaitAsync(this.token).ConfigureAwait(continueOnCapturedContext: false);

                            try
                            {
                                this.readPosition = (ulong)Math.Max(start - 1, 0);
                            }
                            finally
                            {
                                this.semaphore.Release(1);
                            }

                            double downloadProgress;

                            await this.semaphore.WaitAsync(this.token).ConfigureAwait(continueOnCapturedContext: false);

                            try
                            {
                                downloadProgress = (double)this.readPosition / (double)this.data.Length;
                            }
                            finally
                            {
                                this.semaphore.Release(1);
                            }

                            this.RaiseDownloadProgressChanged(downloadProgress);

                            var response = await client.GetAsync(uri, this.token);
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                int read = await stream.ReadAsync(this.data, (int)start, (int)end - (int)start, this.token);

                                if (read != (int)end - (int)start)
                                {
                                    this.logger.Warning("We read not the same value as we expected from url {0}. We read {1}.", uri, read);
                                }
                            }
                        }
                        else
                        {
                            this.logger.Warning("Could not parse start and end range from url {0}", uri);
                        }
                    }

                    this.readPosition = (ulong)this.data.Length;
                    this.IsReady = true;

                    this.RaiseDownloadProgressChanged(1.0d);
                }
                catch (HttpRequestException exception)
                {
                    this.IsFailed = true;

                    if (exception.InnerException is IOException)
                    {
                        this.logger.Debug(exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: HttpRequestException.");
                    }
                    else
                    {
                        this.logger.Error(exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: Exception while reading stream.");
                    }
                }
                catch (IOException exception)
                {
                    this.IsFailed = true;

                    this.logger.Debug(exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: IOException.");
                }
                catch (OperationCanceledException exception)
                {
                    this.IsFailed = true;

                    this.logger.Debug(exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: Downloading task was canceled.");
                    throw;
                }
                catch (Exception exception)
                {
                    this.IsFailed = true;

                    if (this.token.IsCancellationRequested)
                    {
                        this.logger.Debug(exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: Downloading task was canceled .");
                    }
                    else
                    {
                        this.logger.Error(exception, "MemoryRandomAccessStreamMultiStreams.SafeDownloadStream: Exception while reading stream.");
                    }
                }
            }
        }
    }
}