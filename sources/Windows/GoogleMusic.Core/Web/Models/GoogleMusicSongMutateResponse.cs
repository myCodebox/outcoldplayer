// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    using Newtonsoft.Json;

    public class GoogleMusicSongMutateResponse
    {
        [JsonProperty("mutate_response")]
        public GoogleMusicSongMutateItem[] MutateResponse { get; set; }
    }

    public class GoogleMusicSongMutateItem
    {
        public string ResponseCode { get; set; }
    }
}
