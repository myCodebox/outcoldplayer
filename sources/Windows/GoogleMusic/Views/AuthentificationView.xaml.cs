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
        }

        public void ShowCaptcha(string captchaUrl)
        {
        }

        private void SignInClick(object sender, RoutedEventArgs e)
        {
            this.Presenter<AuthentificationPresenter>().SignIn();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
