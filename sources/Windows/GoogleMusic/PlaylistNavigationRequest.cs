// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistNavigationRequest
    {
        public PlaylistNavigationRequest(PlaylistType playlistType, int playlistId)
        {
            this.PlaylistType = playlistType;
            this.PlaylistId = playlistId;
        }

        public PlaylistType PlaylistType { get; set; }

        public int PlaylistId { get; set; }
    }
}