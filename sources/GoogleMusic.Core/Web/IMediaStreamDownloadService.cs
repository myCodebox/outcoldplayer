// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMediaStreamDownloadService
    {
        Task<INetworkRandomAccessStream> GetStreamAsync(string url, CancellationToken token);

        Task<INetworkRandomAccessStream> GetStreamAsync(string[] urls, CancellationToken token);

        Task<IStream> GetCachedStreamAsync(IFile storageFile, CancellationToken token);
    }
}