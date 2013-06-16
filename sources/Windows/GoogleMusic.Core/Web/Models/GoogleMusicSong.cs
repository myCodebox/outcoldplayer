// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleMusicSong
    {
        public string Genre { get; set; }

        public int BeatsPerMinute { get; set; }

        public string AlbumArtistNorm { get; set; }

        public string ArtistNorm { get; set; }

        public string Album { get; set; }

        public double LastPlayed { get; set; }

        public string Type { get; set; }

        public short? Disc { get; set; }

        public string Id { get; set; }

        public string Composer { get; set; }

        public string Title { get; set; }

        public string AlbumArtist { get; set; }

        public string ArtistMatchedId { get; set; }

        public short? TotalTracks { get; set; }

        public bool SubjectToCuration { get; set; }

        public string Name { get; set; }

        public short? TotalDiscs { get; set; }

        public short? Year { get; set; }

        public string TitleNorm { get; set; }

        public string Artist { get; set; }

        public string AlbumNorm { get; set; }

        public short? Track { get; set; }

        public long DurationMillis { get; set; }

        public string MatchedId { get; set; }

        public bool Deleted { get; set; }

        public string Url { get; set; }

        public double CreationDate { get; set; }

        public uint PlayCount { get; set; }

        public short Rating { get; set; }

        public string Comment { get; set; }

        public string AlbumArtUrl { get; set; }

        public string ImageBaseUrl { get; set; }

        public string PlaylistEntryId { get; set; }

        public short Bitrate { get; set; }

        public double RecentTimestamp { get; set; }
    }
}