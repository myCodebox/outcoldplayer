// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml;

    public sealed partial class AuthentificationView : ViewBase, IAuthentificationView
    {
        public AuthentificationView(IDependencyResolverContainer container)
            : base(container)
        {
            this.InitializeComponent();
            this.InitializePresenter<AuthentificationPresenter>();
        }

        public void ShowError(string errorMessage)
        {
            this.ErrorMessageTextBlock.Text = errorMessage;
            this.ErrorMessageTextBlock.Visibility = Visibility.Visible;
        }

        public void ShowCaptcha(string captchaUrl)
        {
        }

        private void SignInClick(object sender, RoutedEventArgs e)
        {
            this.ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
            this.Presenter<AuthentificationPresenter>().SignIn();
        }
    }
}
