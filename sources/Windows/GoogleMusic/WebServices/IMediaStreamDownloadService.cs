// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System.Threading.Tasks;

    public interface IMediaStreamDownloadService
    {
        Task<INetworkRandomAccessStream> GetStreamAsync(string url);
    }
}