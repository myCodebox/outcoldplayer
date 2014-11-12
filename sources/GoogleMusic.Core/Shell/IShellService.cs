// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;
    using System.Threading.Tasks;

    public interface IShellService
    {
        Task LaunchUriAsync(Uri uri);

        Task<bool> HasNetworkConnectionAsync();
    }
}
