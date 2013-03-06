// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    using System.Collections.Generic;

    public class GoogleMusicPlaylists : CommonResponse
    {
        public List<GoogleMusicPlaylist> Playlists { get; set; }

        // TODO: Find what is Magic Playlists?
        // public List<GoogleMusicPlaylist> MagicPlaylists { get; set; }
    }
}