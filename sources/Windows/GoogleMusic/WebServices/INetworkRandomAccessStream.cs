// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System;

    using Windows.Storage.Streams;

    public interface INetworkRandomAccessStream : IRandomAccessStream
    {
        event EventHandler<double> DownloadProgressChanged;

        string ContentType { get; }
    }
}