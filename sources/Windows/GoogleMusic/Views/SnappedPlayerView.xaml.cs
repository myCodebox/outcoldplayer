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
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public interface ISnappedPlayerView : IView
    {
    }

    public sealed partial class SnappedPlayerView : ViewBase, ISnappedPlayerView
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
            this.playerBindingModel = this.GetPresenter<SnappedPlayerViewPresenter>().BindingModel;
            this.playerBindingModel.Subscribe(() => this.playerBindingModel.CurrentSong, this.SongChanged);
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
                    Grid.SetRow(this.adControl, 6);
                    this.SnappedGrid.Children.Add(this.adControl);
                }
            }
        }

        private void AdControlOnErrorOccurred(object sender, AdErrorEventArgs adErrorEventArgs)
        {
            this.logger.LogErrorException(adErrorEventArgs.Error);
        }

        private void RatingOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            var currentSong = this.GetPresenter<SnappedPlayerViewPresenter>().BindingModel.CurrentSong;
            if (currentSong != null)
            {
                if (currentSong.Rating != e.NewValue)
                {
                    this.logger.LogTask(ApplicationBase.Container.Resolve<ISongsService>().UpdateRatingAsync(currentSong, (byte)e.NewValue));
                }
            }
        }

        private void VolumeButtonClick(object sender, RoutedEventArgs e)
        {
            this.VolumePopup.IsOpen = true;
        }

        private void ListDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var songBindingModel = frameworkElement.DataContext as SongBindingModel;
                if (songBindingModel != null)
                {
                    this.GetPresenter<SnappedPlayerViewPresenter>().PlaySong(songBindingModel);
                }
            }
        }
    }
}
