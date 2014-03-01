// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleMusicStationFeed
    {
        public string Kind { get; set; }

        public GoogleMusicStationFeedData Data { get; set; }
    }

    public class GoogleMusicStationFeedData
    {
        public GoogleMusicRadio[] Stations { get; set; }
    }
}
