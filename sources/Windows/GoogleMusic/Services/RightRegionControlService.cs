// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using Microsoft.Advertising.WinRT.UI;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.Shell;
    using OutcoldSolutions.Views;

    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    public class RightRegionControlService 
    {
        private readonly IMainFrameRegionProvider regionProvider;
        private readonly IApplicationSettingViewsService settingsCommands;
        private readonly ILogger logger;

        private HyperlinkButton adReplacementButton;
        private AdControl adControl;
        private Canvas adControlHost;
        private HyperlinkButton adControlRemoveButton;

        public RightRegionControlService(
            ILogManager logManager,
            IMainFrameRegionProvider regionProvider,
            IApplicationSettingViewsService settingsCommands)
        {
            this.logger = logManager.CreateLogger("RightRegionControlService");
            this.regionProvider = regionProvider;
            this.settingsCommands = settingsCommands;

            this.UpdateAdControl();
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
                    this.adReplacementButton = null;
                }
            }
            else
            {
                if (this.adControl == null)
                {
                    this.adControlHost = new Canvas() { Width = 170, Height = 700 };

                    this.adReplacementButton = new HyperlinkButton
                                                   {
                                                       Style = (Style)Application.Current.Resources["RightRegionAdReplacement"]
                                                   };

                    this.adControlHost.Children.Add(this.adReplacementButton);
                    Canvas.SetZIndex(this.adReplacementButton, 0);
                    Canvas.SetTop(this.adReplacementButton, 20);

                    this.adControl = new AdControl
                    {
                        ApplicationId = "8eb9e14b-2133-40db-9500-14eff7c05aab",
                        AdUnitId = "111663",
                        Width = 160,
                        Height = 600,
                        UseStaticAnchor = true
                    };

                    this.adControl.ErrorOccurred += this.AdControlOnErrorOccurred;
                    this.adControlHost.Children.Add(this.adControl);
                    Canvas.SetZIndex(this.adControl, 100);
                    Canvas.SetTop(this.adControl, 20);

                    this.adControlRemoveButton = new HyperlinkButton()
                                                     {
                                                         Content = "Remove Ads",
                                                         HorizontalAlignment = HorizontalAlignment.Center,
                                                         Width = 160,
                                                         HorizontalContentAlignment = HorizontalAlignment.Center
                                                     };

                    this.adControlRemoveButton.Click += this.AdControlRemoveButtonOnClick;
                    this.adControlHost.Children.Add(this.adControlRemoveButton);
                    Canvas.SetTop(this.adControlRemoveButton, 620);

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