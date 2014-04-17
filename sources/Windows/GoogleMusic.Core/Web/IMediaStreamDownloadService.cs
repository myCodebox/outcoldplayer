// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System.Threading;
    using System.Threading.Tasks;

    using Windows.Storage;
    using Windows.Storage.Streams;

    public interface IMediaStreamDownloadService
    {
        Task<INetworkRandomAccessStream> GetStreamAsync(string url, CancellationToken token);

        Task<INetworkRandomAccessStream> GetStreamAsync(string[] urls, CancellationToken token);

        Task<IRandomAccessStream> GetCachedStreamAsync(IStorageFile storageFile, CancellationToken token);
    }
}