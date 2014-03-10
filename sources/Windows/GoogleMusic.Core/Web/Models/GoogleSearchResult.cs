// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleSearchResult
    {
        public string Kind { get; set; }

        public GoogleSearchResultEntry[] Entries { get; set; }
    }

    public class GoogleSearchResultEntry
    {
        public GoogleMusicArtist Artist { get; set; }

        public GoogleMusicAlbum Album { get; set; }

        public GoogleMusicSong Track { get; set; }

        public double Score { get; set; }

        public int Type { get; set; }
    }
}
