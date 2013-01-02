//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views.Settings
{
    using System;

    using OutcoldSolutions.GoogleMusic.BindingModels.Settings;

    using Windows.Storage;
    using Windows.System;
    using Windows.UI.ApplicationSettings;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    public sealed partial class SupportView : UserControl
    {
        public SupportView()
        {
            this.InitializeComponent();

            this.DataContext = new SupportBindingModel();

            this.LogFolder.Text = ApplicationData.Current.LocalFolder.Path;
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            var popup = this.Parent as Popup;
            if (popup != null)
            {
                popup.IsOpen = false;
            }

            SettingsPane.Show();
        }

        private void UserVoiceClick(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://gmusic.uservoice.com"));
        }

        private void SendEmailClick(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("mailto:gMusic@outcoldman.com"));
        }

        private void LogFolderGotFocus(object sender, RoutedEventArgs e)
        {
            this.LogFolder.SelectAll();
        }
    }
}
