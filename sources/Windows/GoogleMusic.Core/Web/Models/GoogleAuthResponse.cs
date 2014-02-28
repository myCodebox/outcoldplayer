// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    using System;
    using System.Net;

    public class GoogleAuthResponse
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

        public CookieCollection CookieCollection { get; private set; }

        public string Auth { get; private set; }

        public static GoogleAuthResponse SuccessResponse(CookieCollection cookieCollection, string auth)
        {
            if (cookieCollection == null)
            {
                throw new ArgumentNullException("cookieCollection");
            }

            return new GoogleAuthResponse()
                       {
                           Success = true,
                           CookieCollection = cookieCollection,
                           Auth = auth
                       };
        }

        public static GoogleAuthResponse ErrorResponse(ErrorResponseCode error)
        {
            return new GoogleAuthResponse()
                       {
                           Success = false,
                           Error = error
                       };
        }
    }
}