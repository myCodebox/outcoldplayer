// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;

    using Windows.UI.Core;
    using Windows.UI.Xaml;

    using OutcoldSolutions.GoogleMusic.Presenters;

    public interface IPlayerView : IView
    {
    }

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
                    this.RatingControl.Value = playerBindingModel.CurrentSong == null
                        ? 0
                        : playerBindingModel.CurrentSong.Rating;
                }));
        
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            VolumePopup.IsOpen = true;
        }
    }
}
