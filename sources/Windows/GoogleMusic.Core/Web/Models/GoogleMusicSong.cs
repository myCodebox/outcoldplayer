// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class ArtRef
    {
        public string Url { get; set; }
    }

    public class GoogleMusicSong
    {
        public string Kind { get; set; }

        public string Id { get; set; }

        public string ClientId { get; set; }

        public double CreationTimestamp { get; set; }

        public double LastModifiedTimestamp { get; set; }

        public double RecentTimestamp { get; set; }

        public bool Deleted { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public string Composer { get; set; }

        public string Album { get; set; }

        public string AlbumArtist { get; set; }

        public int? Year { get; set; }

        public string Comment { get; set; }

        public int? TrackNumber { get; set; }

        public string Genre { get; set; }

        public long DurationMillis { get; set; }

        public int BeatsPerMinute { get; set; }

        public ArtRef[] AlbumArtRef { get; set; }

        public ArtRef[] ArtistArtRef { get; set; }

        public uint PlayCount { get; set; }

        public int? TotalTrackCount { get; set; }

        public int? DiscNumber { get; set; }

        public int? TotalDiscCount { get; set; }

        public int Rating { get; set; }

        public string EstimatedSize { get; set; }

        public string[] ArtistId { get; set; }

        public string StoreId { get; set; }

        public string AlbumId { get; set; }

        public string Nid { get; set; }

        public int TrackType { get; set; }

        public int ContentType { get; set; }

        public bool TrackAvailableForSubscription { get; set; }

        public bool TrackAvailableForPurchase { get; set; }

        public bool AlbumAvailableForPurchase { get; set; }
    }
}