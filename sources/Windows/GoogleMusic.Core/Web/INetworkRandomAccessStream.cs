// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;

    using Windows.Storage.Streams;

    public interface INetworkRandomAccessStream : IRandomAccessStreamWithContentType
    {
        event EventHandler<double> DownloadProgressChanged;
    }
}