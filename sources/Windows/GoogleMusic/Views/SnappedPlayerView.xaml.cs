//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Advertising.WinRT.UI;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.Views;

    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public interface ISnappedPlayerView : IView
    {
        Task ScrollIntoCurrentSongAsync();
    }

    public sealed partial class SnappedPlayerView : ViewBase, ISnappedPlayerView
    {
        private readonly ILogger logger;
        private AdControl adControl;
        private SnappedPlayerBindingModel playerBindingModel;
        private HyperlinkButton adReplacementButton;

        public SnappedPlayerView()
        {
            this.InitializeComponent();

            this.logger = ApplicationBase.Container.Resolve<ILogManager>().CreateLogger("SnappedPlayerView");

            InAppPurchases.LicenseChanged += this.UpdateAdControl;
            this.UpdateAdControl();
            
            this.Loaded += this.OnLoaded;
        }

        public async Task ScrollIntoCurrentSongAsync()
        {
            await Task.Run(
                async () =>
                {
                    if (this.playerBindingModel != null 
                        && this.playerBindingModel.CurrentSong != null
                        && this.playerBindingModel.SongsBindingModel.Songs != null)
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, this.UpdateLayout);
                        var currentSongBindingModel = this.playerBindingModel.SongsBindingModel.Songs
                            .FirstOrDefault(x => string.Equals(x.Metadata.SongId, this.playerBindingModel.CurrentSong.Metadata.SongId, StringComparison.Ordinal));
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.ListView.ScrollIntoView(currentSongBindingModel));
                    }
                });
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.OnLoaded;
            this.playerBindingModel = this.GetPresenter<SnappedPlayerViewPresenter>().BindingModel;

            this.UpdateCurrentSongRating();
            this.playerBindingModel.Subscribe(() => this.playerBindingModel.CurrentSong, (o, args) => this.UpdateCurrentSongRating());
        }

        private void UpdateCurrentSongRating()
        {
            this.RatingControl.Value = this.playerBindingModel.CurrentSong == null
                                           ? 0
                                           : this.playerBindingModel.CurrentSong.Rating;
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

                    this.SnappedGrid.Children.Remove(this.adReplacementButton);
                    this.adReplacementButton = null;
                }
            }
            else
            {
                if (this.adControl == null)
                {
                    this.adReplacementButton = new HyperlinkButton
                    {
                        Style = (Style)Application.Current.Resources["SnappedViewAdReplacement"]
                    };

                    Grid.SetRow(this.adReplacementButton, 6);
                    Canvas.SetZIndex(this.adReplacementButton, 0);
                    this.SnappedGrid.Children.Add(this.adReplacementButton);

                    this.adControl = new AdControl
                    {
                        ApplicationId = "8eb9e14b-2133-40db-9500-14eff7c05aab",
                        AdUnitId = "156438",
                        Width = 300,
                        Height = 250,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        UseStaticAnchor = true
                    };

                    this.adControl.ErrorOccurred += this.AdControlOnErrorOccurred;
                    Grid.SetRow(this.adControl, 6);
                    Canvas.SetZIndex(this.adControl, 100);
                    this.SnappedGrid.Children.Add(this.adControl);
                }
            }
        }

        private void AdControlOnErrorOccurred(object sender, AdErrorEventArgs adErrorEventArgs)
        {
            this.logger.Debug("Exception from ad control {0}", adErrorEventArgs.Error.Message);
        }

        private void RatingOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            var currentSong = this.GetPresenter<SnappedPlayerViewPresenter>().BindingModel.CurrentSong;
            if (currentSong != null)
            {
                if (currentSong.Rating != e.NewValue)
                {
                    this.logger.LogTask(ApplicationBase.Container.Resolve<ISongsService>().UpdateRatingAsync(currentSong.Metadata, (byte)e.NewValue));
                }
            }
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

        private async void CurrentSongButtonClick(object sender, RoutedEventArgs e)
        {
            await this.ScrollIntoCurrentSongAsync();
        }
    }
}
