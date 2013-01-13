// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.LastFm
{
    public enum GetSessionError
    {
        Unknown = 0,
        InvalidToken = 4,
        TokenIsNotAuthorized = 14,
        TokenHasExpired = 15,
        InvalidService = 2,
        InvalidMethod = 3,
        AuthentificationFailed = 4,
        InvalidFormat = 5,
        InvalidParameters = 6,
        InvalidResourceSpecified = 7,
        OperationFailed = 8,
        InvalidSessionKey = 9,
        InvalidApiKey = 10,
        ServiceOffline = 11,
        InvalidMethodSignature = 13,
        TemporaryError = 16,
        SuspendedApiKey = 26,
        RateLimitExceeded = 29
    }
}