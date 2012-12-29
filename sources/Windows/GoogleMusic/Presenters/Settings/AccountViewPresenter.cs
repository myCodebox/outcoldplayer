// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using OutcoldSolutions.GoogleMusic.BindingModels.Settings;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class AccountViewPresenter : ViewPresenterBase<IView>
    {
        private readonly IUserDataStorage userDataStorage;

        public AccountViewPresenter(
            IDependencyResolverContainer container, 
            IView view,
            IUserDataStorage userDataStorage)
            : base(container, view)
        {
            this.userDataStorage = userDataStorage;
            this.BindingModel = new AccountViewBindingModel();
            this.ForgetAccountCommand = new DelegateCommand(this.ForgetAccount);

            var userInfo = this.userDataStorage.GetUserInfo(retrievePassword: false);
            if (userInfo != null)
            {
                this.BindingModel.AccountName = userInfo.Email;
            }
        }

        public AccountViewBindingModel BindingModel { get; private set; }

        public DelegateCommand ForgetAccountCommand { get; private set; }

        private void ForgetAccount()
        {
            this.userDataStorage.ClearUserInfo();
            this.BindingModel.AccountName = null;
            this.BindingModel.Message = "All stored information were cleared. Next time you will be asked to provide Google Account email and password to continue use application.";
        }
    }
}