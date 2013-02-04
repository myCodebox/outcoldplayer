// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    using System;

    public class AddSongResp : CommonResponse
    {
        public Guid PlaylistId { get; set; }

        public SongIdResp[] SongIds { get; set; }
    }
}