// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleLoginResponse
    {
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

        public bool Success { get; private set; }

        public string Auth { get; private set; }

        public ErrorResponseCode? Error { get; private set; }

        public string CaptchaToken { get; private set; }

        public string CaptchaUrl { get; private set; }

        public static GoogleLoginResponse SuccessResponse(string auth)
        {
            return new GoogleLoginResponse()
                       {
                           Success = true,
                           Auth = auth
                       };
        }

        public static GoogleLoginResponse ErrorResponse(ErrorResponseCode error, string captchToken = null, string captchaUrl = null)
        {
            return new GoogleLoginResponse()
                       {
                           Success = false,
                           Error = error,
                           CaptchaToken = captchToken,
                           CaptchaUrl = captchaUrl
                       };
        }
    }
}