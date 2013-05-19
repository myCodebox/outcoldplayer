// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Threading.Tasks;

    using Windows.Storage;
    using Windows.Storage.Streams;

    public interface INetworkRandomAccessStream : IRandomAccessStreamWithContentType
    {
        event EventHandler<double> DownloadProgressChanged;

        bool IsReady { get; }

        Task DownloadAsync();

        Task SaveToFileAsync(IStorageFile file);
    }
}