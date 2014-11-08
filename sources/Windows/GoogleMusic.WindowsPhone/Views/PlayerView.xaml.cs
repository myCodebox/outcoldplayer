// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using System.Linq;

    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Presenters;

    public sealed partial class PlayerView : ViewBase, IPlayerView
    {
        public PlayerView()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var playerBindingModel = this.GetPresenter<PlayerViewPresenter>().BindingModel;
            playerBindingModel.Subscribe(() => playerBindingModel.CurrentSong, async (o, args) => await this.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, 
                () =>
                {
                    var ratingControl = VisualTreeHelperEx.GetVisualChild<Rating>(this.ContentControl, "RatingControl");
                    if (ratingControl != null)
                    {
                        ratingControl.Value = playerBindingModel.CurrentSong == null
                            ? 0
                            : playerBindingModel.CurrentSong.Rating;
                    }
                }));
        }

        private void VolumeButton_OnClick(object sender, RoutedEventArgs e)
        {
            var popup = VisualTreeHelperEx.GetVisualChild<Popup>(this.ContentControl, "VolumePopup");
            if (popup != null)
            {
                popup.IsOpen = true;
            }
        }
    }
}
