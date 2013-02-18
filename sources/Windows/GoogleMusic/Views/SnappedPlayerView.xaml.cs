//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Microsoft.Advertising.WinRT.UI;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Web;

    using Windows.ApplicationModel.Store;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class SnappedPlayerView : UserControl
    {
        private readonly ILogger logger;

        private AdControl adControl;

        public SnappedPlayerView()
        {
            this.InitializeComponent();

            this.logger = ApplicationBase.Container.Resolve<ILogManager>().CreateLogger("SnappedPlayerView");

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
                    Grid.SetRow(this.adControl, 7);
                    this.SnappedGrid.Children.Add(this.adControl);
                }
            }
        }

        private void AddToQueue(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.TryUnsnap())
            {
                ApplicationBase.Container.Resolve<INavigationService>().NavigateTo<IStartPageView>();
            }
        }

        private void RatingOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            var playerViewPresenter = this.DataContext as PlayerViewPresenter;
            if (playerViewPresenter != null)
            {
                var currentSong = playerViewPresenter.BindingModel.CurrentSong;
                if (currentSong != null)
                {
                    if (currentSong.Rating != e.NewValue)
                    {
                        ApplicationBase.Container.Resolve<ISongWebService>()
                                       .UpdateRatingAsync(currentSong.Metadata.Id, e.NewValue).ContinueWith(
                            async t =>
                            {
                                if (t.IsCompleted && !t.IsFaulted && t.Result != null)
                                {
                                    if (this.logger.IsDebugEnabled)
                                    {
                                        this.logger.Debug("Rating update completed for song: {0}.", currentSong.Metadata.Id);
                                    }

                                    foreach (var songUpdate in t.Result.Songs)
                                    {
                                        if (this.logger.IsDebugEnabled)
                                        {
                                            this.logger.Debug("Song updated: {0}, Rate: {1}.", songUpdate.Id, songUpdate.Rating);
                                        }

                                        if (songUpdate.Id == currentSong.Metadata.Id)
                                        {
                                            var songRatingResp = songUpdate;
                                            await ApplicationBase.Container.Resolve<IDispatcher>().RunAsync(() => { currentSong.Rating = songRatingResp.Rating; });
                                        }
                                    }
                                }
                                else
                                {
                                    this.logger.Debug("Failed to update rating for song: {0}.", currentSong.Metadata.Id);
                                    if (t.IsFaulted && t.Exception != null)
                                    {
                                        this.logger.LogErrorException(t.Exception);
                                    }
                                }
                            });
                    }
                }
            }
        }
    }
}
