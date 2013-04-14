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

        public int Type { get; set; }

        public ushort? Disc { get; set; }

        public string Id { get; set; }

        public string Composer { get; set; }

        public string Title { get; set; }

        public string AlbumArtist { get; set; }

        public string ArtistMatchedId { get; set; }

        public ushort? TotalTracks { get; set; }

        public bool SubjectToCuration { get; set; }

        public string Name { get; set; }

        public ushort? TotalDiscs { get; set; }

        public ushort? Year { get; set; }

        public string TitleNorm { get; set; }

        public string Artist { get; set; }

        public string AlbumNorm { get; set; }

        public ushort? Track { get; set; }

        public long DurationMillis { get; set; }

        public string MatchedId { get; set; }

        public bool Deleted { get; set; }

        public string Url { get; set; }

        public double CreationDate { get; set; }

        public ushort PlayCount { get; set; }

        public byte Rating { get; set; }

        public string Comment { get; set; }

        public string AlbumArtUrl { get; set; }

        public string PlaylistEntryId { get; set; }

        public ushort Bitrate { get; set; }

        public double RecentTimestamp { get; set; }
    }
}