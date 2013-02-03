//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Microsoft.Advertising.WinRT.UI;

    using Windows.ApplicationModel.Store;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class SnappedPlayerView : UserControl
    {
        private AdControl adControl;

        public SnappedPlayerView()
        {
            this.InitializeComponent();
#if DEBUG
            CurrentAppSimulator.LicenseInformation.LicenseChanged += this.UpdateAdControl;
#else
            CurrentApp.LicenseInformation.LicenseChanged += this.UpdateAdControl;
#endif
            this.UpdateAdControl();
        }

        private bool IsAdFree()
        {
#if DEBUG
            return (CurrentAppSimulator.LicenseInformation.ProductLicenses.ContainsKey("AdFreeUnlimited")
                && CurrentAppSimulator.LicenseInformation.ProductLicenses["AdFreeUnlimited"].IsActive)
                || (CurrentAppSimulator.LicenseInformation.ProductLicenses.ContainsKey("Ultimate")
                && CurrentAppSimulator.LicenseInformation.ProductLicenses["Ultimate"].IsActive);
#else
            return (CurrentApp.LicenseInformation.ProductLicenses.ContainsKey("AdFreeUnlimited")
                && CurrentApp.LicenseInformation.ProductLicenses["AdFreeUnlimited"].IsActive)
                || (CurrentApp.LicenseInformation.ProductLicenses.ContainsKey("Ultimate")
                && CurrentApp.LicenseInformation.ProductLicenses["Ultimate"].IsActive);
#endif
        }

        private void UpdateAdControl()
        {
            if (this.IsAdFree())
            {
                if (this.adControl != null)
                {
                    this.SnappedGrid.Children.Remove(this.adControl);
                    this.adControl = null;
                }
            }
            else
            {
                if (this.adControl == null)
                {
                    this.adControl = new AdControl
                    {
                        ApplicationId = "8eb9e14b-2133-40db-9500-14eff7c05aab",
                        AdUnitId = "111664",
                        Width = 292,
                        Height = 60,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        UseStaticAnchor = true
                    };
                    Grid.SetRow(this.adControl, 6);
                    this.SnappedGrid.Children.Add(this.adControl);
                }
            }
        }

        private void AddToQueue(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.TryUnsnap())
            {
                App.Container.Resolve<INavigationService>().NavigateTo<IStartView>();
            }
        }
    }
}
