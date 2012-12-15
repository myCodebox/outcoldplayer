// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Net;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.WebServices;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class AuthentificationPresenter : ViewPresenterBase<IAuthentificationView>
    {
        private readonly IClientLoginService clientLoginWebService;
        private readonly IUserDataStorage userDataStorage;

        private string captchaToken = null;

        public AuthentificationPresenter(
            IDependencyResolverContainer container, 
            IAuthentificationView view,
            IClientLoginService clientLoginWebService,
            IUserDataStorage userDataStorage)
            : base(container, view)
        {
            this.clientLoginWebService = clientLoginWebService;
            this.userDataStorage = userDataStorage;
            this.BindingModel = new UserAuthentificationBindingModel();

            var userInfo = this.userDataStorage.GetUserInfo();
            if (userInfo != null)
            {
                this.BindingModel.Email = userInfo.Email;
                this.BindingModel.RememberAccount = true;
            }
        }

        public UserAuthentificationBindingModel BindingModel { get; private set; }

        public void SignIn()
        {
            var email = this.BindingModel.Email;
            var password = this.BindingModel.Password;
            var rememberPassword = this.BindingModel.RememberAccount;
            var token = this.captchaToken;

            // TODO: Implement captcha

            if (string.IsNullOrEmpty(email) 
                || string.IsNullOrEmpty(password))
            {
                this.View.ShowError("Please provide email and password first.");
            }
            else
            {
                this.captchaToken = null;
                this.clientLoginWebService.LoginAsync(email, password)
                    .ContinueWith(
                    loginTask =>
                        {
                            var loginResponse = loginTask.Result;
                            if (loginResponse.IsOk)
                            {
                                if (rememberPassword)
                                {
                                    this.userDataStorage.SaveUserInfo(new UserInfo(email, password));
                                }

                                var auth = loginResponse.GetAuth();
                                this.clientLoginWebService.GetCookieAsync(auth)
                                    .ContinueWith(
                                        cookieTask =>
                                            {
                                                var cookieResponse = cookieTask.Result;
                                                if (cookieResponse.HttpWebResponse.StatusCode == HttpStatusCode.OK)
                                                {
                                                    this.userDataStorage.SaveCookies(
                                                        cookieResponse.HttpWebResponse.ResponseUri, 
                                                        cookieResponse.HttpWebResponse.Cookies);
                                                }
                                                else
                                                {
                                                    this.Logger.Error("Cannot get cookie. Web Response Status code is '{0}'.", cookieResponse.HttpWebResponse.StatusCode);

                                                    // Better error
                                                    this.View.ShowError("Cannot get request from service. Check internet connection.");
                                                }
                                            });
                            }
                            else
                            {
                                var errorResponse = loginResponse.AsError();
                                string errorMessage = GetErrorMessage(errorResponse.Code);
                                this.View.ShowError(errorMessage);

                                if (errorResponse.Code == GoogleLoginResponse.ErrorResponseCode.CaptchaRequired)
                                {
                                    this.View.ShowCaptcha(errorResponse.CaptchaUrl);
                                    this.captchaToken = errorResponse.CaptchaToken;
                                }
                            }
                        });
            }
        }

        public void Cancel()
        {
            
        }

        private string GetErrorMessage(GoogleLoginResponse.ErrorResponseCode errorResponseCode)
        {
            switch (errorResponseCode)
            {
                case GoogleLoginResponse.ErrorResponseCode.BadAuthentication:
                    return "The login request used a username or password that is not recognized.";
                case GoogleLoginResponse.ErrorResponseCode.NotVerified:
                    return "The account email address has not been verified. The user will need to access their Google account directly to resolve the issue before logging in using a non-Google application.";
                case GoogleLoginResponse.ErrorResponseCode.TermsNotAgreed:
                    return "The user has not agreed to terms. The user will need to access their Google account directly to resolve the issue before logging in using a non-Google application.";
                case GoogleLoginResponse.ErrorResponseCode.CaptchaRequired:
                    return "A CAPTCHA is required.";
                case GoogleLoginResponse.ErrorResponseCode.Unknown:
                    return "The error is unknown or unspecified.";
                case GoogleLoginResponse.ErrorResponseCode.AccountDeleted:
                    return "The user account has been deleted.";
                case GoogleLoginResponse.ErrorResponseCode.AccountDisabled:
                    return "The user account has been disabled.";
                case GoogleLoginResponse.ErrorResponseCode.ServiceDisabled:
                    return "The user's access to the specified service has been disabled.";
                case GoogleLoginResponse.ErrorResponseCode.ServiceUnavailable:
                    return "The service is not available; try again later.";
                default:
                    throw new NotSupportedException("Value is not supported: " + errorResponseCode.ToString());
            }
        }
    }
}