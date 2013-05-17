// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System.Threading.Tasks;

    public interface IMediaStreamDownloadService
    {
        Task<INetworkRandomAccessStream> GetStreamAsync(string url);

        Task<INetworkRandomAccessStream> GetStreamAsync(string[] urls);
    }
}