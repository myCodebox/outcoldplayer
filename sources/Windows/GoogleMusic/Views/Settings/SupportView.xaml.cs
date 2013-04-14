//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views.Settings
{
    using System;

    using OutcoldSolutions.GoogleMusic.BindingModels.Settings;
    using OutcoldSolutions.Views;

    using Windows.Storage;
    using Windows.System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class SupportView : UserControl, IApplicationSettingsContent
    {
        public SupportView()
        {
            this.InitializeComponent();

            this.DataContext = new SupportBindingModel();

            this.LogFolder.Text = ApplicationData.Current.LocalFolder.Path;
        }

        private void UserVoiceClick(object sender, RoutedEventArgs e)
        {
            var taskResult = Launcher.LaunchUriAsync(new Uri("https://gmusic.uservoice.com"));
        }
        
        private void LogFolderGotFocus(object sender, RoutedEventArgs e)
        {
            this.LogFolder.SelectAll();
        }

        private void TwitterFollowClick(object sender, RoutedEventArgs e)
        {
            var taskResult = Launcher.LaunchUriAsync(new Uri("https://twitter.com/gMusicW"));
        }
    }
}
