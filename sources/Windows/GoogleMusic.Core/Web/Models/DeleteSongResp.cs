// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class DeleteSongResp : CommonResponse
    {
        public string ListId { get; set; }

        public string[] DeleteIds { get; set; }
    }
}