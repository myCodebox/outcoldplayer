//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Settings
{
    using Windows.ApplicationModel.Store;
    using Windows.UI.ApplicationSettings;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    public sealed partial class UpgradeView : UserControl
    {
        public UpgradeView()
        {
            this.InitializeComponent();

#if DEBUG
            this.AdFreePackageButton.IsEnabled =
                !CurrentAppSimulator.LicenseInformation.ProductLicenses["AdFreeUnlimited"].IsActive;

            this.UltimatePackageButton.IsEnabled =
                !CurrentAppSimulator.LicenseInformation.ProductLicenses["Ultimate"].IsActive;
#else
            this.AdFreePackageButton.IsEnabled =
                !CurrentApp.LicenseInformation.ProductLicenses["AdFreeUnlimited"].IsActive;

            this.UltimatePackageButton.IsEnabled =
                !CurrentApp.LicenseInformation.ProductLicenses["Ultimate"].IsActive;
#endif
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();

            SettingsPane.Show();
        }

        private void AdFreeUnlimitedPackage(object sender, RoutedEventArgs e)
        {
            if (this.AdFreePackageButton.IsEnabled)
            {
                this.AdFreePackageButton.IsEnabled = false;
#if DEBUG
                var taskResult = CurrentAppSimulator.RequestProductPurchaseAsync("AdFreeUnlimited", false);
#else
                CurrentApp.RequestProductPurchaseAsync("AdFreeUnlimited", false);
#endif
                this.Close();
            }
        }

        private void UltimatePackage(object sender, RoutedEventArgs e)
        {
            if (this.UltimatePackageButton.IsEnabled)
            {
                this.UltimatePackageButton.IsEnabled = false;
#if DEBUG
                var taskResult = CurrentAppSimulator.RequestProductPurchaseAsync("Ultimate", false);
#else
                CurrentApp.RequestProductPurchaseAsync("Ultimate", false);
#endif
                this.Close();
            }
        }

        private void Close()
        {
            var popup = this.Parent as Popup;
            if (popup != null)
            {
                popup.IsOpen = false;
            }
        }
    }
}
