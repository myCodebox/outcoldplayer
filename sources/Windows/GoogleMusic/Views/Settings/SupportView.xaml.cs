//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views.Settings
{
    using System;

    using Windows.Storage;
    using Windows.System;
    using Windows.UI.Xaml;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    public sealed partial class SupportView : ViewBase, IApplicationSettingsContent
    {
        public SupportView()
        {
            this.InitializeComponent();

            this.LogFolder.Text = ApplicationData.Current.LocalFolder.Path;
        }

        private void UserVoiceClick(object sender, RoutedEventArgs e)
        {
            this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri("https://outcoldplayer.uservoice.com")).AsTask());
        }
        
        private void LogFolderGotFocus(object sender, RoutedEventArgs e)
        {
            this.LogFolder.SelectAll();
        }

        private void TwitterFollowClick(object sender, RoutedEventArgs e)
        {
            this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri("https://twitter.com/outcoldplayer")).AsTask());
        }

        private void GotoPayPal(object sender, RoutedEventArgs e)
        {
            this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=3SLEL7WWJFSDC&lc=US&item_name=outcoldplayer&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHosted")).AsTask());
        }
    }
}
