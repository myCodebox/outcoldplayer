// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleMusicErrorResponse
    {
        public GoogleMusicWebError Error { get; set; }
    }

    public class GoogleMusicWebError
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public GoolgeMusicError[] Errors { get; set; }
    }

    public class GoolgeMusicError
    {
        public string Domain { get; set; }

        public string Reason { get; set; }

        public string Message { get; set; }
    }
}
