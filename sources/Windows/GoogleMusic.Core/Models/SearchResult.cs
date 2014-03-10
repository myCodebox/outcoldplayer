// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;

    public class SearchResult
    {
        public SearchResult(string searchString)
        {
            this.SearchString = searchString;
        }

        public string SearchString { get; set; }

        public IList<Song> Songs { get; set; }

        public IList<IPlaylist> Artists { get; set; } 

        public IList<IPlaylist> Albums { get; set; } 

        public IList<IPlaylist> Genres { get; set; } 

        public IList<IPlaylist> UserPlaylists { get; set; } 

        public IList<IPlaylist> RadioStations { get; set; } 
    }
}
