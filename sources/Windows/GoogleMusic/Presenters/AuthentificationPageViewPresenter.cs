// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Presenters;

    public class AuthentificationPageViewPresenter : PagePresenterBase<IAuthentificationPageView>
    {
        private readonly INavigationService navigationService;
        private readonly IGoogleAccountService googleAccountService;
        private readonly IAuthentificationService authentificationService;

        public AuthentificationPageViewPresenter(
            INavigationService navigationService,
            IGoogleAccountService googleAccountService,
            IAuthentificationService authentificationService)
        {
            this.navigationService = navigationService;
            this.googleAccountService = googleAccountService;
            this.authentificationService = authentificationService;
            this.BindingModel = new AuthentificationPageViewBindingModel();

            this.SignInCommand = new DelegateCommand(this.SignIn, () => !this.BindingModel.IsSigningIn);

            this.BindingModel.Subscribe(() => this.BindingModel.IsSigningIn, (sender, args) => this.SignInCommand.RaiseCanExecuteChanged());

            var userInfo = this.googleAccountService.GetUserInfo();
            if (userInfo != null)
            {
                this.Logger.Debug("Found user info. Trying to set user email.");
                this.BindingModel.Email = userInfo.Email;
                this.BindingModel.RememberAccount = userInfo.RememberAccount;
            }
        }

        public AuthentificationPageViewBindingModel BindingModel { get; private set; }

        public DelegateCommand SignInCommand { get; private set; }

        private async void SignIn()
        {
            this.BindingModel.IsSigningIn = true;

            this.BindingModel.ErrorMessage = null;

            var email = this.BindingModel.Email;
            var password = this.BindingModel.Password;
            var rememberPassword = this.BindingModel.RememberAccount;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                this.Logger.Warning("Cannot login. Email or password are not provided.");
                this.BindingModel.ErrorMessage = "Please provide email and password first.";
            }
            else
            {
                var userInfo = new UserInfo(email, password) { RememberAccount = rememberPassword };

                this.Logger.Debug("Trying to proceed authentification.");

                AuthentificationService.AuthentificationResult result = null;

                try
                {
                    result = await this.authentificationService.CheckAuthentificationAsync(userInfo);
                }
                catch (Exception exception)
                {
                    this.Logger.LogErrorException(exception);
                }

                if (result != null && result.Succeed)
                {
                    this.Logger.Debug("Authentification succeded.");

                    if (!rememberPassword)
                    {
                        this.Logger.Debug("User asked to not save user information. Removing user info and password.");
                        this.googleAccountService.ClearUserInfo();
                    }

                    this.Logger.Debug("Saving user info and password.");
                    this.googleAccountService.SetUserInfo(userInfo);

                    this.navigationService.NavigateTo<IProgressLoadingView>();
                }
                else
                {
                    if (result != null)
                    {
                        this.Logger.Debug("Authentification is not succeded. {0}.", result.ErrorMessage);
                        this.BindingModel.ErrorMessage = result.ErrorMessage;
                    }
                    else
                    {
                        this.BindingModel.ErrorMessage = "Could not authentificate. Please check network connection.";
                    }
                }
            }

            this.BindingModel.IsSigningIn = false;
        }
    }
}