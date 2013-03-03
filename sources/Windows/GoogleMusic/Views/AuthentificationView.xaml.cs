// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.Views;

    using Windows.System;
    using Windows.UI.Xaml;

    public sealed partial class AuthentificationPageView : PageViewBase, IAuthentificationView
    {
        private AuthentificationPresenter presenter;

        public AuthentificationPageView()
        {
            this.InitializeComponent();
        }

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
                                this.Logger.LogTask(t);

                                this.ProgressRing.IsActive = false;
                                this.SetLoginLayoutIsEnabled(isEnabled: true);
                            }
                            else
                            {
                                this.NavigationService.NavigateTo<IProgressLoadingView>();
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
            this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri("https://play.google.com/music")).AsTask());
        }
    }
}
