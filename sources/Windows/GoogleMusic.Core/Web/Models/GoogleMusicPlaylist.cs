// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleMusicPlaylist
    {
        public string Kind { get; set; }

        public string Id { get; set; }

        public double CreationTimestramp { get; set; }

        public double LastModifiedTimestamp { get; set; }

        public double RecentTimestamp { get; set; }

        public bool Deleted { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string ShareToken { get; set; }

        public string OwnerName { get; set; }

        public string OwnerProfilePhotoUrl { get; set; }

        public bool AccessControlled { get; set; }
    }
}