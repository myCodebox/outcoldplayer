//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Settings
{
    using OutcoldSolutions.Views;

    using Windows.UI.ApplicationSettings;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls.Primitives;

    public sealed partial class AccountPageView : PageViewBase, IPopupView
    {
        public AccountPageView()
        {
            this.InitializeComponent();
        }

        public void Close()
        {
            var popup = this.Parent as Popup;
            if (popup != null)
            {
                popup.IsOpen = false;
            }
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
    }
}
