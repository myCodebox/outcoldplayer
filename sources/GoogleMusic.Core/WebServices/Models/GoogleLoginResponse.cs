// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices.Models
{
    using System;
    using System.Net;

    public class GoogleLoginResponse
    {
        private readonly GoogleWebResponse googleWebResponse;

        public GoogleLoginResponse(GoogleWebResponse googleWebResponse)
        {
            this.googleWebResponse = googleWebResponse;
        }

        public enum ErrorResponseCode
        {
            Unknown,
            BadAuthentication,
            NotVerified,
            TermsNotAgreed,
            CaptchaRequired,
            AccountDeleted,
            AccountDisabled,
            ServiceDisabled,
            ServiceUnavailable
        }

        public bool IsOk
        {
            get
            {
                return this.googleWebResponse.HttpWebResponse.StatusCode == HttpStatusCode.OK;
            }
        }

        public string GetAuth()
        {
            if (!this.IsOk)
            {
                throw new NotSupportedException("Cannot get Auth if response is not OK.");
            }

            string auth = null;

            var body = this.googleWebResponse.GetAsPlainLines();

            foreach (var bodyLine in body)
            {
                if (string.Equals(bodyLine.Key, "Auth", StringComparison.OrdinalIgnoreCase))
                {
                    auth = bodyLine.Value;
                }
            }

            return auth;
        }

        public ErrorResponse AsError()
        {
            ErrorResponseCode code = ErrorResponseCode.Unknown;
            string captchaToken = null;
            string captchaUrl = null;

            if (this.googleWebResponse.HttpWebResponse.StatusCode == HttpStatusCode.Forbidden)
            {
                var body = this.googleWebResponse.GetAsPlainLines();

                foreach (var bodyLine in body)
                {
                    if (string.Equals(bodyLine.Key, "Error", StringComparison.OrdinalIgnoreCase))
                    {
                        Enum.TryParse(bodyLine.Value, out code);
                    }
                    else if (string.Equals(bodyLine.Key, "CaptchaToken", StringComparison.OrdinalIgnoreCase))
                    {
                        captchaToken = bodyLine.Value;
                    }
                    else if (string.Equals(bodyLine.Key, "CaptchaUrl", StringComparison.OrdinalIgnoreCase))
                    {
                        captchaUrl = bodyLine.Value;
                    }
                }
            }

            return new ErrorResponse(code, captchaToken, captchaUrl);
        }

        public class ErrorResponse
        {
            public ErrorResponse(ErrorResponseCode code, string captchaToken, string captchaUrl)
            {
                this.Code = code;
                this.CaptchaToken = captchaToken;
                this.CaptchaUrl = captchaUrl;
            }

            public ErrorResponseCode Code { get; private set; }

            public string CaptchaToken { get; private set; }

            public string CaptchaUrl { get; private set; }
        }
    }
}