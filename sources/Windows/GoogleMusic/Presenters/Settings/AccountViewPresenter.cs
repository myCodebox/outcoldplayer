// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using OutcoldSolutions.GoogleMusic.BindingModels.Settings;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Settings;

    public class AccountViewPresenter : ViewPresenterBase<ISettingsView>
    {
        private readonly IGoogleAccountService googleAccountService;
        private readonly IGoogleMusicSessionService sessionService;

        public AccountViewPresenter(
            IDependencyResolverContainer container,
            ISettingsView view,
            IGoogleAccountService googleAccountService,
            IGoogleMusicSessionService sessionService)
            : base(container, view)
        {
            this.googleAccountService = googleAccountService;
            this.sessionService = sessionService;
            this.BindingModel = new AccountViewBindingModel();
            this.ForgetAccountCommand = new DelegateCommand(this.ForgetAccount);
            this.SignOutCommand = new DelegateCommand(this.SignOutAccount);

            var userInfo = this.googleAccountService.GetUserInfo();
            if (userInfo != null)
            {
                this.BindingModel.AccountName = userInfo.Email;
                this.BindingModel.IsRemembered = userInfo.RememberAccount;
            }

            this.BindingModel.HasSession = this.sessionService.GetSession().IsAuthenticated;
        }

        public AccountViewBindingModel BindingModel { get; private set; }

        public DelegateCommand ForgetAccountCommand { get; private set; }

        public DelegateCommand SignOutCommand { get; private set; }

        private void ForgetAccount()
        {
            this.googleAccountService.ClearUserInfo();
            this.BindingModel.AccountName = null;
            this.BindingModel.Message = "All stored information were cleared. Next time you will be asked to provide Google Account email and password to continue use application.";
        }

        private void SignOutAccount()
        {
            this.sessionService.ClearSession();
            this.View.Hide();
        }
    }
}