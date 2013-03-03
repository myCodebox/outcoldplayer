// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;

    using Microsoft.Advertising.WinRT.UI;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Views;

    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Store;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class RightRegionControlService 
    {
        private readonly INavigationService navigationService;
        private readonly IMainFrameRegionProvider regionProvider;
        private readonly ISettingsCommands settingsCommands;
        private readonly ILogger logger;

        private AdControl adControl;
        private StackPanel adControlHost;
        private HyperlinkButton adControlRemoveButton;

        public RightRegionControlService(
            INavigationService navigationService,
            ILogManager logManager,
            IMainFrameRegionProvider regionProvider,
            ISettingsCommands settingsCommands)
        {
            this.logger = logManager.CreateLogger("RightRegionControlService");
            this.navigationService = navigationService;
            this.regionProvider = regionProvider;
            this.settingsCommands = settingsCommands;
#if DEBUG
            this.LoadFakeAdds();
            CurrentAppSimulator.LicenseInformation.LicenseChanged += this.UpdateAdControl;
#else
            CurrentApp.LicenseInformation.LicenseChanged += this.UpdateAdControl;
#endif

            this.navigationService.NavigatedTo += (sender, args) =>
                {
                    var isVisible = !(args.View is IAuthentificationView) 
                                    && !(args.View is IInitPageView)
                                    && !(args.View is IProgressLoadingView);

                    this.regionProvider.SetVisibility(MainFrameRegion.Right, isVisible);
                };
        }

#if DEBUG
        private async void LoadFakeAdds()
        {
            StorageFolder proxyDataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Resources");
            StorageFile proxyFile = await proxyDataFolder.GetFileAsync("in-app-purchase.xml");

            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
        }
#endif

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
                    this.regionProvider.SetContent(MainFrameRegion.Right, null);
                    this.adControl.ErrorOccurred -= this.AdControlOnErrorOccurred;
                    this.adControlRemoveButton.Click -= this.AdControlRemoveButtonOnClick;
                    this.adControl = null;
                    this.adControlHost = null;
                    this.adControlRemoveButton = null;
                }
            }
            else
            {
                if (this.adControl == null)
                {
                    this.adControlHost = new StackPanel();

                    this.adControl = new AdControl
                    {
                        ApplicationId = "8eb9e14b-2133-40db-9500-14eff7c05aab",
                        AdUnitId = "111663",
                        Width = 160,
                        Height = 600,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(0, 20, 10, 0),
                        UseStaticAnchor = true
                    };

                    this.adControl.ErrorOccurred += this.AdControlOnErrorOccurred;
                    this.adControlHost.Children.Add(this.adControl);

                    this.adControlRemoveButton = new HyperlinkButton()
                                                     {
                                                         Content = "Remove Ads",
                                                         HorizontalAlignment = HorizontalAlignment.Center
                                                     };

                    this.adControlRemoveButton.Click += this.AdControlRemoveButtonOnClick;
                    this.adControlHost.Children.Add(this.adControlRemoveButton);

                    this.regionProvider.SetContent(MainFrameRegion.Right, this.adControlHost);
                }
            }
        }

        private void AdControlRemoveButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.settingsCommands.ActivateSettings("upgrade");
        }

        private void AdControlOnErrorOccurred(object sender, AdErrorEventArgs adErrorEventArgs)
        {
            this.logger.LogErrorException(adErrorEventArgs.Error);
        }
    }
}