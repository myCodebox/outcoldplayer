// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class AddSongResp
    {
        public string PlaylistId { get; set; }

        public SongIdResp[] SongIds { get; set; }
    }
}