// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class RatingResp
    {
        public SongRatingResp[] Songs { get; set; }

        public bool Success { get; set; }
    }
}