// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleMusicSharedPlaylistEntriesResponse
    {
        public string Kind { get; set; }

        public GoogleMusicSharedPlaylist[] Entries { get; set; }
    }

    public class GoogleMusicSharedPlaylist
    {
        public string ResponseCode { get; set; }

        public string ShareToken { get; set; }

        public GoogleMusicPlaylistEntry[] PlaylistEntry { get; set; }
    }
}
