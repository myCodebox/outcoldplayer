// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    using Newtonsoft.Json;

    public class GoogleMusicPlaylistEntriesBatchResponse
    {
        [JsonProperty("mutate_response")]
        public GoogleMusicPlaylistEntriesBatchEntry[] MutateResponse { get; set; }
    }

    public class GoogleMusicPlaylistEntriesBatchEntry
    {
        public string Id { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("response_code")]
        public string ResponseCode { get; set; }
    }
}
