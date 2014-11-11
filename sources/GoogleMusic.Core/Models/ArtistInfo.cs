// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;

    public class ArtistInfo
    {
        public Artist Artist { get; set; }

        public IList<Album> GoogleAlbums { get; set; }

        public IList<Song> TopSongs { get; set; }

        public IList<Artist> RelatedArtists { get; set; } 
    }
}
