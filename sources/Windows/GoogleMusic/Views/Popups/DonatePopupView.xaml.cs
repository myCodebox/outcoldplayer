// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    using System;

    using Windows.System;
    using Windows.UI.Xaml;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    public interface IDonatePopupView : IPopupView
    {
    }

    public class DonateCloseEventArgs : EventArgs
    {
        public DonateCloseEventArgs(bool later)
        {
            this.Later = later;
        }

        public bool Later { get; set; }
    }

    public sealed partial class DonatePopupView : PopupViewBase, IDonatePopupView
    {
        public DonatePopupView()
        {
            this.InitializeComponent();
        }

        private void GotoPayPal(object sender, RoutedEventArgs e)
        {
            this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=3SLEL7WWJFSDC&lc=US&item_name=outcoldplayer&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHosted")).AsTask());
        }

        private void GotoRating(object sender, RoutedEventArgs e)
        {
            this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=47286outcoldman.gMusic_z1q2m7teapq4y")).AsTask());
        }

        private void Later(object sender, RoutedEventArgs e)
        {
            this.Close(new DonateCloseEventArgs(true));  
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close(new DonateCloseEventArgs(false));
        }

        private void UserVoiceClick(object sender, RoutedEventArgs e)
        {
            this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri("https://outcoldplayer.uservoice.com")).AsTask());
        }
    }
}
