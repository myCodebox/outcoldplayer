//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Settings
{
    using OutcoldSolutions.GoogleMusic.Presenters.Settings;

    using Windows.UI.ApplicationSettings;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls.Primitives;

    public sealed partial class AccountView : ViewBase, IPopupView
    {
        public AccountView()
        {
            this.InitializePresenter<AccountViewPresenter>();
            this.InitializeComponent();
        }

        public void Hide()
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
