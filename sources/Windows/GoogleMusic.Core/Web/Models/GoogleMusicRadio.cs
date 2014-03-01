// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleMusicRadio
    {
        public string Kind { get; set; }

        public string Id { get; set; }

        public string ClientId { get; set; }

        public double LastModifiedTimestamp { get; set; }

        public double RecentTimestamp { get; set; }

        public bool Deleted { get; set; }

        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public UrlRef[] ImageUrls { get; set; }

        public GoogleMusicRadioSeed Seed { get; set; }

        public GoogleMusicSong[] Tracks { get; set; }
    }

    public class GoogleMusicRadioSeed
    {
        public string Kind { get; set; }

        public string TrackLockerId { get; set; }

        public string TrackId { get; set; }

        public string ArtistId { get; set; }

        public string AlbumId { get; set; }

        public int SeedType { get; set; }

        public GoogleMusicRadioMetadataSeed MetadataSeed { get; set; }
    }

    public class GoogleMusicRadioMetadataSeed
    {
        public string Kind { get; set; }

        public GoogleMusicSong Track { get; set; }

        public GoogleMusicArtist Artist { get; set; }
    }
}
