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
        private readonly IUserDataStorage userDataStorage;

        public AccountViewPresenter(
            IDependencyResolverContainer container,
            ISettingsView view,
            IUserDataStorage userDataStorage)
            : base(container, view)
        {
            this.userDataStorage = userDataStorage;
            this.BindingModel = new AccountViewBindingModel();
            this.ForgetAccountCommand = new DelegateCommand(this.ForgetAccount);
            this.SignOutCommand = new DelegateCommand(this.SignOutAccount);

            var userInfo = this.userDataStorage.GetUserInfo();
            if (userInfo != null)
            {
                this.BindingModel.AccountName = userInfo.Email;
                this.BindingModel.IsRemembered = userInfo.RememberAccount;
            }

            this.BindingModel.HasSession = this.userDataStorage.GetUserSession() != null;
        }

        public AccountViewBindingModel BindingModel { get; private set; }

        public DelegateCommand ForgetAccountCommand { get; private set; }

        public DelegateCommand SignOutCommand { get; private set; }

        private void ForgetAccount()
        {
            this.userDataStorage.ClearUserInfo();
            this.BindingModel.AccountName = null;
            this.BindingModel.Message = "All stored information were cleared. Next time you will be asked to provide Google Account email and password to continue use application.";
        }

        private void SignOutAccount()
        {
            this.userDataStorage.ClearSession();
            this.View.Hide();
        }
    }
}