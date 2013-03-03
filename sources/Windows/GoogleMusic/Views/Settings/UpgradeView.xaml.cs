//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Settings
{
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.UI.ApplicationSettings;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    public sealed partial class UpgradeView : UserControl
    {
        public UpgradeView()
        {
            this.InitializeComponent();

            this.AdFreePackageButton.IsEnabled = !InAppPurchases.IsActive(InAppPurchases.AdFreeUnlimitedInAppPurchase);
            this.UltimatePackageButton.IsEnabled = !InAppPurchases.IsActive(InAppPurchases.UltimateInAppPurchase);
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

                // TODO: Log result
                InAppPurchases.RequestPurchase(InAppPurchases.AdFreeUnlimitedInAppPurchase);
                this.Close();
            }
        }

        private void UltimatePackage(object sender, RoutedEventArgs e)
        {
            if (this.UltimatePackageButton.IsEnabled)
            {
                this.UltimatePackageButton.IsEnabled = false;

                // TODO: Log result
                InAppPurchases.RequestPurchase(InAppPurchases.UltimateInAppPurchase);
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
