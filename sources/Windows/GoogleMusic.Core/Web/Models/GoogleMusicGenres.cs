// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleMusicGenres
    {
        public string Kind { get; set; }

        public GoogleMusicGenre[] Genres { get; set; }
    }

    public class GoogleMusicGenre
    {
        public string Kind { get; set; }

        public string Name { get; set; }

        public string Id { get; set; }

        public string ParentId { get; set; }

        public string[] Children { get; set; }

        public UrlRef[] Images { get; set; }
    }
}
