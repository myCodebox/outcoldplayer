//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Settings
{
    using OutcoldSolutions.GoogleMusic.Presenters.Settings;

    using Windows.UI.ApplicationSettings;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls.Primitives;

    public sealed partial class AccountView : ViewBase, IView
    {
        public AccountView()
        {
            this.InitializePresenter<AccountViewPresenter>();
            this.InitializeComponent();
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
