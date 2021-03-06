﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class AuthentificationPopupViewPresenter : ViewPresenterBase<IAuthentificationPopupView>
    {
        private readonly IApplicationResources resources;
        private readonly IGoogleAccountService googleAccountService;
        private readonly IAuthentificationService authentificationService;

        private readonly IAnalyticsService analyticsService;

        private string captchaToken;

        public AuthentificationPopupViewPresenter(
            IApplicationResources resources,
            IGoogleAccountService googleAccountService,
            IAuthentificationService authentificationService,
            IAnalyticsService analyticsService)
        {
            this.resources = resources;
            this.googleAccountService = googleAccountService;
            this.authentificationService = authentificationService;
            this.analyticsService = analyticsService;
            this.BindingModel = new AuthentificationPageViewBindingModel();

            this.SignInCommand = new DelegateCommand(this.SignIn, () => !this.BindingModel.IsSigningIn);

            this.BindingModel.Subscribe(() => this.BindingModel.IsSigningIn, (sender, args) => this.SignInCommand.RaiseCanExecuteChanged());
        }

        public AuthentificationPageViewBindingModel BindingModel { get; private set; }

        public DelegateCommand SignInCommand { get; private set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var userInfo = this.googleAccountService.GetUserInfo();
            if (userInfo != null)
            {
                this.Logger.Debug("Found user info. Trying to set user email.");
                this.BindingModel.Email = userInfo.Email;
                this.BindingModel.RememberAccount = userInfo.RememberAccount;
            }
        }

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
                this.BindingModel.ErrorMessage = this.resources.GetString("Authorization_Error_UserNameAndPassword");
            }
            else
            {
                var userInfo = new UserInfo(email, password) { RememberAccount = rememberPassword };

                this.Logger.Debug("Trying to proceed authentification.");

                AuthentificationService.AuthentificationResult result = null;

                try
                {
                    result = await this.authentificationService.CheckAuthentificationAsync(userInfo,
                        this.BindingModel.IsCaptchaRequired ? this.captchaToken : null,
                        this.BindingModel.IsCaptchaRequired ? this.BindingModel.Captcha : null,
                        /* forceCaptcha */ false);
                }
                catch (OperationCanceledException exception)
                {
                    this.Logger.Debug(exception, "Operation was canceled.");
                }
                catch (Exception exception)
                {
                    this.Logger.Error(exception, "Exception while tried to authentificate.");
                }

                if (result != null && result.Succeed)
                {
                    this.Logger.Debug("Authentification succeded.");

                    if (!rememberPassword)
                    {
                        this.Logger.Debug("User asked to not save user information. Removing user info and password.");
                        this.googleAccountService.ClearUserInfo();
                    }

                    this.analyticsService.SendEvent("Authentification", "SavePassword", rememberPassword.ToString());

                    this.Logger.Debug("Saving user info and password.");
                    this.googleAccountService.SetUserInfo(userInfo);

                    this.View.Close();
                }
                else
                {
                    if (result != null)
                    {
                        this.Logger.Debug("Authentification is not succeded. {0}.", result.ErrorMessage);
                        this.BindingModel.ErrorMessage = result.ErrorMessage;
                        this.BindingModel.IsCaptchaRequired = result.IsCaptchaRequired;
                        this.BindingModel.CaptchaUrl = string.IsNullOrEmpty(result.CaptchaUrl) ? null : new Uri(result.CaptchaUrl);
                        this.BindingModel.Captcha = string.Empty;
                        this.captchaToken = result.CaptchaToken;
                    }
                    else
                    {
                        this.BindingModel.ErrorMessage = this.resources.GetString("Authorization_Error_UnexpectedError");
                    }
                }
            }

            this.BindingModel.IsSigningIn = false;
        }
    }
}