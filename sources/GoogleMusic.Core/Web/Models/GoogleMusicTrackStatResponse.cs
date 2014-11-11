// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    using Newtonsoft.Json;

    public class GoogleMusicTrackStatResponse
    {
        public GoogleMusicTrackStat[] Responses { get; set; }
    }

    public class GoogleMusicTrackStat
    {
        public string Id { get; set; }

        [JsonProperty("response_code")]
        public string ResponseCode { get; set; }
    }
}
