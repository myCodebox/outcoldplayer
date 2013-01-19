// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class StatusResp : CommonResponse
    {
        public int AvailableTracks { get; set; }

        public int TotalTracks { get; set; }
    }
}