// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Settings;

    using Windows.UI.ApplicationSettings;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    public class SettingsCommands : ISettingsCommands
    {
        private const double SettingsWidth = 346;

        private readonly IMediaElemenetContainerView mediaElemenetContainerView;

        private Popup settingsPopup;

        public SettingsCommands(IMediaElemenetContainerView mediaElemenetContainerView)
        {
            this.mediaElemenetContainerView = mediaElemenetContainerView;
            SettingsPane.GetForCurrentView().CommandsRequested += this.CommandsRequested;
        }

        private void CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var cmd = new SettingsCommand(
                "account",
                "Account",
                (x) => this.CreatePopup(new AccountView()));

            args.Request.ApplicationCommands.Add(cmd);

            cmd = new SettingsCommand(
                "support",
                "Support",
                (x) => this.CreatePopup(new SupportView()));

            args.Request.ApplicationCommands.Add(cmd);
        }

        private void CreatePopup(UserControl view)
        {
            this.settingsPopup = new Popup();
            this.settingsPopup.Closed += this.OnPopupClosed;
            Window.Current.Activated += this.OnWindowActivated;
            this.settingsPopup.IsLightDismissEnabled = true;
            this.settingsPopup.Width = SettingsWidth;
            this.settingsPopup.Height = Window.Current.Bounds.Height;

            view.Height = this.settingsPopup.Height;
            view.Width = this.settingsPopup.Width;

            this.settingsPopup.Child = view;
            this.settingsPopup.SetValue(Canvas.LeftProperty, Window.Current.Bounds.Width - SettingsWidth);
            this.settingsPopup.SetValue(Canvas.TopProperty, 0);
            this.settingsPopup.IsOpen = true;

            this.mediaElemenetContainerView.HideAd();
        }

        private void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                this.settingsPopup.IsOpen = false;
            }
        }

        private void OnPopupClosed(object sender, object e)
        {
            Window.Current.Activated -= this.OnWindowActivated;
            this.settingsPopup.Closed -= this.OnPopupClosed;
            this.settingsPopup = null;

            this.mediaElemenetContainerView.ShowAd();
        }
    }
}