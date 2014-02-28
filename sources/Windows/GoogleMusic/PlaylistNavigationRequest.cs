// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistNavigationRequest
    {
        public PlaylistNavigationRequest(PlaylistType playlistType, string playlistId, string songId = null)
        {
            this.PlaylistType = playlistType;
            this.PlaylistId = playlistId;
            this.SongId = songId;
        }

        public PlaylistType PlaylistType { get; set; }

        public string PlaylistId { get; set; }

        public string SongId { get; set; }
    }
}