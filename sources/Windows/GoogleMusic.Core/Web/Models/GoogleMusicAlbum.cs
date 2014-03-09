// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleMusicAlbum
    {
        public string Kind { get; set; }

        public string Name { get; set; }

        public string AlbumArtist { get; set; }

        public string AlbumArtRef { get; set; }

        public string AlbumId { get; set; }

        public string Artist { get; set; }

        public string[] ArtistId { get; set; }

        public string Description { get; set; }

        public int Year { get; set; }

        public GoogleMusicSong[] Tracks { get; set; }
    }
}