// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using Microsoft.Advertising.WinRT.UI;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Shell;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class RightRegionControlService 
    {
        private readonly IMainFrameRegionProvider regionProvider;
        private readonly IApplicationSettingViewsService settingsCommands;
        private readonly ILogger logger;

        private AdControl adControl;
        private StackPanel adControlHost;
        private HyperlinkButton adControlRemoveButton;

        public RightRegionControlService(
            ILogManager logManager,
            IMainFrameRegionProvider regionProvider,
            IApplicationSettingViewsService settingsCommands)
        {
            this.logger = logManager.CreateLogger("RightRegionControlService");
            this.regionProvider = regionProvider;
            this.settingsCommands = settingsCommands;

            InAppPurchases.LicenseChanged += this.UpdateAdControl;
        }

        private void UpdateAdControl()
        {
            if (InAppPurchases.HasFeature(GoogleMusicFeatures.AdFree))
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
            this.settingsCommands.Show("upgrade");
        }

        private void AdControlOnErrorOccurred(object sender, AdErrorEventArgs adErrorEventArgs)
        {
            this.logger.Debug("Exception from ad control: {0}.", adErrorEventArgs.Error.Message);
        }
    }
}