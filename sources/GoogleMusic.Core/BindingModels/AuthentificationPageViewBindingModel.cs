// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    public class AuthentificationPageViewBindingModel : BindingModelBase
    {
        private string email;
        private string password;
        private bool rememberAccount;
        private string errorMessage;
        private bool isSigningIn;
        private bool isCaptchaRequired;
        private string captcha;
        private Uri captchaUrl;

        public string Email
        {
            get
            {
                return this.email;
            }

            set
            {
                this.SetValue(ref this.email, value);
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }

            set
            {
                this.SetValue(ref this.password, value);
            }
        }

        public bool RememberAccount
        {
            get
            {
                return this.rememberAccount;
            }

            set
            {
                this.SetValue(ref this.rememberAccount, value);
            }
        }

        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }

            set
            {
                this.SetValue(ref this.errorMessage, value);
            }
        }

        public bool IsSigningIn
        {
            get
            {
                return this.isSigningIn;
            }

            set
            {
                this.SetValue(ref this.isSigningIn, value);
            }
        }

        public bool IsCaptchaRequired
        {
            get
            {
                return this.isCaptchaRequired;
            }

            set
            {
                this.SetValue(ref this.isCaptchaRequired, value);
            }
        }

        public string Captcha
        {
            get
            {
                return this.captcha;
            }

            set
            {
                this.SetValue(ref this.captcha, value);
            }
        }

        public Uri CaptchaUrl
        {
            get
            {
                return this.captchaUrl;
            }

            set
            {
                this.SetValue(ref this.captchaUrl, value);
            }
        }
    }
}