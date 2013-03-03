// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Presenters;

    public class AuthentificationPresenter : PagePresenterBase<IAuthentificationView>
    {
        private readonly IGoogleAccountService googleAccountService;
        private readonly IAuthentificationService authentificationService;

        public AuthentificationPresenter(
            IDependencyResolverContainer container, 
            IGoogleAccountService googleAccountService,
            IAuthentificationService authentificationService)
            : base(container)
        {
            this.googleAccountService = googleAccountService;
            this.authentificationService = authentificationService;
            this.BindingModel = new UserAuthentificationBindingModel();

            var userInfo = this.googleAccountService.GetUserInfo();
            if (userInfo != null)
            {
                this.Logger.Debug("Found user info. Trying to set user email.");
                this.BindingModel.Email = userInfo.Email;
                this.BindingModel.RememberAccount = userInfo.RememberAccount;
            }
        }

        public UserAuthentificationBindingModel BindingModel { get; private set; }

        public async Task<bool> LogInAsync()
        {
            this.BindingModel.ErrorMessage = null;

            var email = this.BindingModel.Email;
            var password = this.BindingModel.Password;
            var rememberPassword = this.BindingModel.RememberAccount;

            // TODO: Implement captcha
            if (string.IsNullOrEmpty(email) 
                || string.IsNullOrEmpty(password))
            {
                this.Logger.Warning("Cannot login. Email or password is not provided.");
                this.BindingModel.ErrorMessage = "Please provide email and password first.";// CoreResources.Login_UserNameAndPassword;
                return false;
            }
            else
            {
                var userInfo = new UserInfo(email, password) { RememberAccount = rememberPassword };

                this.Logger.Debug("Trying to proceed authentification.");

                var result = await this.authentificationService.CheckAuthentificationAsync(userInfo);

                if (result.Succeed)
                {
                    this.Logger.Debug("Authentification succeded.");

                    if (!rememberPassword)
                    {
                        this.Logger.Debug("User asked to not save user information. Removing user info and password.");
                        this.googleAccountService.ClearUserInfo();
                    }

                    this.Logger.Debug("Saving user info and password.");
                    this.googleAccountService.SetUserInfo(userInfo);
                }
                else
                {
                    this.Logger.Debug("Authentification is not succeded. {0}.", result.ErrorMessage);
                    this.BindingModel.ErrorMessage = result.ErrorMessage;
                }

                return result.Succeed;
            }
        }
    }
}