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

    public class AuthentificationPresenter : ViewPresenterBase<IAuthentificationView>
    {
        private readonly IUserDataStorage userDataStorage;
        private readonly IAuthentificationService authentificationService;

        private string captchaToken = null;

        public AuthentificationPresenter(
            IDependencyResolverContainer container, 
            IAuthentificationView view,
            IUserDataStorage userDataStorage,
            IAuthentificationService authentificationService)
            : base(container, view)
        {
            this.userDataStorage = userDataStorage;
            this.authentificationService = authentificationService;
            this.BindingModel = new UserAuthentificationBindingModel();

            var userInfo = this.userDataStorage.GetUserInfo();
            if (userInfo != null)
            {
                this.Logger.Debug("Found user info. Trying to set user email.");
                this.BindingModel.Email = userInfo.Email;
                this.BindingModel.RememberAccount = true;
            }
        }

        public UserAuthentificationBindingModel BindingModel { get; private set; }

        public async Task<bool> LogInAsync()
        {
            this.BindingModel.ErrorMessage = null;

            var email = this.BindingModel.Email;
            var password = this.BindingModel.Password;
            var rememberPassword = this.BindingModel.RememberAccount;
            var token = this.captchaToken;

            // TODO: Implement captcha
            if (string.IsNullOrEmpty(email) 
                || string.IsNullOrEmpty(password))
            {
                this.Logger.Warning("Cannot login. Email or password is not provided.");
                this.BindingModel.ErrorMessage = "Please provide email and password first.";
                return false;
            }
            else
            {
                this.captchaToken = null;
                var userInfo = new UserInfo(email, password);

                this.Logger.Debug("Trying to proceed authentification.");

                var result = await this.authentificationService.CheckAuthentificationAsync(userInfo);

                if (result.Succeed)
                {
                    this.Logger.Debug("Authentification succeded.");

                    if (rememberPassword)
                    {
                        this.Logger.Debug("Saving user info and password.");
                        this.userDataStorage.SaveUserInfo(userInfo);
                    }
                    else
                    {
                        this.Logger.Debug("User asked to not save user information. Removing user info and password.");
                        this.userDataStorage.ClearUserInfo();
                    }
                }
                else
                {
                    this.Logger.Debug("Authentification is not succeded. {0}.", result.ErrorMessage);
                    this.BindingModel.ErrorMessage = result.ErrorMessage;
                    
                    if (result.Captcha != null)
                    {
                        this.View.ShowCaptcha(result.Captcha.CaptchaUrl);
                        this.captchaToken = result.Captcha.CaptchaToken;
                    }
                }

                return result.Succeed;
            }
        }
    }
}