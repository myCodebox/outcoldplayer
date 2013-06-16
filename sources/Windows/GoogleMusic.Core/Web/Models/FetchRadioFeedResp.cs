// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class FetchRadioFeedResp : CommonResponse
    {
        public string Id { get; set; }

        public GoogleMusicSong[] Track { get; set; }
    }
}