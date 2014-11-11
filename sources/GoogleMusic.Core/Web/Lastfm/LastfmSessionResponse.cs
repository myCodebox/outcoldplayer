// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Lastfm
{
    public class LastfmSessionResponse
    {
        public enum ErrorCode
        {
            Unknown = 0,

            InvalidService = 2,

            InvalidMethod = 3,

            InvalidToken = 4,

            InvalidFormat = 5,

            InvalidParameters = 6,

            InvalidResourceSpecified = 7,

            OperationFailed = 8,

            InvalidSessionKey = 9,

            InvalidApiKey = 10,

            ServiceOffline = 11,

            InvalidMethodSignature = 13,

            TokenIsNotAuthorized = 14,

            TokenHasExpired = 15,

            TemporaryError = 16,

            SuspendedApiKey = 26,

            RateLimitExceeded = 29
        }

        public bool Success { get; private set; }

        public LastfmSession LastfmSession { get; private set; }

        public ErrorCode? Error { get; private set; }

        public static LastfmSessionResponse SuccessResponse(LastfmSession session)
        {
            return new LastfmSessionResponse() { Success = true, LastfmSession = session };
        }

        public static LastfmSessionResponse ErrorResponse(ErrorCode error)
        {
            return new LastfmSessionResponse() { Success = false, Error = error };
        }
    }
}