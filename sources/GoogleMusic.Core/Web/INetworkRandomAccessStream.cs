// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Threading.Tasks;

    public interface INetworkRandomAccessStream : IStream
    {
        event EventHandler<double> DownloadProgressChanged;

        bool IsReady { get; }

        bool IsFailed { get; }

        Task DownloadAsync();

        Task SaveToFileAsync(IFile file);
    }
}