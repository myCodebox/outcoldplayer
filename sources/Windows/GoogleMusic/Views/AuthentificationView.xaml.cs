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
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public sealed partial class AuthentificationPageView : PageViewBase, IAuthentificationView
    {
        private AuthentificationPresenter presenter;

        public AuthentificationPageView()
        {
            this.InitializeComponent();
        }

        public event EventHandler Succeed;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<AuthentificationPresenter>();
        }

        private void SignInClick(object sender, RoutedEventArgs e)
        {
            if (this.SignInButton.IsEnabled)
            {
                this.SetLoginLayoutIsEnabled(isEnabled: false);
                this.ProgressRing.IsActive = true;
                this.presenter.LogInAsync().ContinueWith(
                    t =>
                        {
                            if (!t.IsCompleted || t.IsFaulted || !t.Result)
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
