// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class StatusResp
    {
        public int AvailableTracks { get; set; }

        public int TotalTracks { get; set; }

        public string Success { get; set; }

        public string ReloadXsrf { get; set; }
    }
}