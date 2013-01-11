// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.System;
    using Windows.UI.Xaml;

    public sealed partial class AuthentificationView : ViewBase, IAuthentificationView
    {
        public AuthentificationView()
        {
            this.InitializeComponent();
            this.InitializePresenter<AuthentificationPresenter>();
        }

        public event EventHandler Succeed;

        private void SignInClick(object sender, RoutedEventArgs e)
        {
            if (this.SignInButton.IsEnabled)
            {
                this.SetLoginLayoutIsEnabled(isEnabled: false);
                this.ProgressRing.IsActive = true;
                this.Presenter<AuthentificationPresenter>().LogInAsync().ContinueWith(
                    t =>
                        {
                            if (!t.Result)
                            {
                                this.ProgressRing.IsActive = false;
                                this.SetLoginLayoutIsEnabled(isEnabled: true);
                            }
                            else
                            {
                                this.RaiseSucceed();
                            }
                        },
                    TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void SetLoginLayoutIsEnabled(bool isEnabled)
        {
            this.SignInButton.IsEnabled = isEnabled;
            this.EmailTextBox.IsEnabled = isEnabled;
            this.PasswordTextBox.IsEnabled = isEnabled;
            this.RememberCheckBox.IsEnabled = isEnabled;
        }

        private void GotoGoogleMusic(object sender, RoutedEventArgs e)
        {
            var tResult = Launcher.LaunchUriAsync(new Uri("https://play.google.com/music"));
        }

        private void RaiseSucceed()
        {
            var handler = this.Succeed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
