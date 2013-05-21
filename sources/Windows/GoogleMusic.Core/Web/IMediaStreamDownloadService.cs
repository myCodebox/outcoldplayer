// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System.Threading.Tasks;

    using Windows.Storage;
    using Windows.Storage.Streams;

    public interface IMediaStreamDownloadService
    {
        Task<INetworkRandomAccessStream> GetStreamAsync(string url);

        Task<INetworkRandomAccessStream> GetStreamAsync(string[] urls);

        Task<IRandomAccessStream> GetCachedStreamAsync(IStorageFile storageFile);
    }
}