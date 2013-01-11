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

        public ErrorResponseCode? Error { get; private set; }

        public static GoogleLoginResponse SuccessResponse()
        {
            return new GoogleLoginResponse()
                       {
                           Success = true
                       };
        }

        public static GoogleLoginResponse ErrorResponse(ErrorResponseCode error)
        {
            return new GoogleLoginResponse()
                       {
                           Success = false,
                           Error = error
                       };
        }
    }
}