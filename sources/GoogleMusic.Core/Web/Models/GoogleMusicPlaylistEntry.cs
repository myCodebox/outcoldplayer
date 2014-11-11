// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleMusicPlaylistEntry
    {
        public string Kind { get; set; }

        public string Id { get; set; }

        public string ClientId { get; set; }

        public string PlaylistId { get; set; }

        public string AbsolutePosition { get; set; }

        public string TrackId { get; set; }

        public double CreationTimestamp { get; set; }

        public double LastModifiedTimestamp { get; set; }

        public bool Deleted { get; set; }

        public int Source { get; set; }

        public GoogleMusicSong Track { get; set; }
    }
}
