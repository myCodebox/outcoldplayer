// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.WebServices.Models
{
    using System.Collections.Generic;

    public class GoogleMusicPlaylists
    {
        public List<GoogleMusicPlaylist> Playlists { get; set; }

        public List<GoogleMusicPlaylist> MagicPlaylists { get; set; }
    }
}