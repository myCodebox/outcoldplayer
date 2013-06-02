// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistNavigationRequest
    {
        public PlaylistNavigationRequest(PlaylistType playlistType, int playlistId, int? songId = null)
        {
            this.PlaylistType = playlistType;
            this.PlaylistId = playlistId;
            this.SongId = songId;
        }

        public PlaylistType PlaylistType { get; set; }

        public int PlaylistId { get; set; }

        public int? SongId { get; set; }
    }
}