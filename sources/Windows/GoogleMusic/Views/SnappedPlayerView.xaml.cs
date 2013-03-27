//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.ComponentModel;

    using Microsoft.Advertising.WinRT.UI;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class SnappedPlayerView : UserControl
    {
        private readonly ILogger logger;

        private AdControl adControl;

        private PlayerBindingModel playerBindingModel;

        public SnappedPlayerView()
        {
            this.InitializeComponent();

            this.logger = ApplicationBase.Container.Resolve<ILogManager>().CreateLogger("SnappedPlayerView");

            InAppPurchases.LicenseChanged += this.UpdateAdControl;
            this.UpdateAdControl();

            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.OnLoaded;
            var playerViewPresenter = this.DataContext as PlayerViewPresenter;
            if (playerViewPresenter != null)
            {
                this.playerBindingModel = playerViewPresenter.BindingModel;
                this.playerBindingModel.Subscribe(() => this.playerBindingModel.CurrentSong, this.SongChanged);
            }
        }

        private void SongChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var currentSong = this.playerBindingModel.CurrentSong;
            this.Rating.Value = (currentSong != null) ? currentSong.Rating : 0;
        }

        private void UpdateAdControl()
        {
            if (InAppPurchases.HasFeature(GoogleMusicFeatures.AdFree))
            {
                if (this.adControl != null)
                {
                    this.SnappedGrid.Children.Remove(this.adControl);
                    this.adControl.ErrorOccurred -= this.AdControlOnErrorOccurred;
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
                    this.adControl.ErrorOccurred += this.AdControlOnErrorOccurred;
                    Grid.SetRow(this.adControl, 7);
                    this.SnappedGrid.Children.Add(this.adControl);
                }
            }
        }

        private void AdControlOnErrorOccurred(object sender, AdErrorEventArgs adErrorEventArgs)
        {
            this.logger.LogErrorException(adErrorEventArgs.Error);
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
                        this.logger.LogTask(ApplicationBase.Container.Resolve<ISongsService>().UpdateRatingAsync(currentSong, (byte)e.NewValue));
                    }
                }
            }
        }
    }
}
