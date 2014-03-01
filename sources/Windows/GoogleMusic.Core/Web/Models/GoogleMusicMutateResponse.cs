// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    using Newtonsoft.Json;

    public class GoogleMusicMutateResponse
    {
        [JsonProperty("mutate_response")]
        public GoogleMusicMutateResponseItem[] MutateResponse { get; set; }
    }

    public class GoogleMusicMutateResponseItem
    {
        public string Id { get; set; }

        public string ClientId { get; set; }

        [JsonProperty("response_code")]
        public string ResponseCode { get; set; }

        public GoogleMusicRadio Station { get; set; }
    }
}
