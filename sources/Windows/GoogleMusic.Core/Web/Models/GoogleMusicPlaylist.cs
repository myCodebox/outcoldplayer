// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    using System.Collections.Generic;

    public class GoogleMusicPlaylist
    {
        public string Title { get; set; }

        public string PlaylistId { get; set; }

        public double RequestTime { get; set; }

        public string ContinuationToken { get; set; }

        public bool DifferentialUpdate { get; set; }

        public List<GoogleMusicSong> Playlist { get; set; }

        public bool Continuation { get; set; }
    }
}