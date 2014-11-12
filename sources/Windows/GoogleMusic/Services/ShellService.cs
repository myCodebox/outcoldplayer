// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Threading.Tasks;
    using Windows.Networking.Connectivity;
    using Windows.System;
    using OutcoldSolutions.GoogleMusic.Shell;

    public class ShellService : IShellService
    {
        public Task LaunchUriAsync(Uri uri)
        {
            return Launcher.LaunchUriAsync(uri).AsTask();
        }

        public Task<bool> HasNetworkConnectionAsync()
        {
            return Task.Run(() =>
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile != null)
                {
                    var networkConnectivityLevel = profile.GetNetworkConnectivityLevel();
                    if (networkConnectivityLevel != NetworkConnectivityLevel.ConstrainedInternetAccess
                        && networkConnectivityLevel != NetworkConnectivityLevel.InternetAccess)
                    {
                        return false;
                    }
                }

                return true;
            });
        }
    }
}
