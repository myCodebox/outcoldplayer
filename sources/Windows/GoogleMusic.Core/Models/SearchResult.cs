// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;

    public class SearchResultEntity
    {
        public double Score { get; set; }

        public IPlaylist Playlist { get; set; }

        public Song Song { get; set; }
    }

    public class SearchResult
    {
        public SearchResult(string searchString)
        {
            this.SearchString = searchString;
        }

        public string SearchString { get; set; }

        public List<SearchResultEntity> Songs { get; set; }

        public List<SearchResultEntity> Artists { get; set; }

        public List<SearchResultEntity> Albums { get; set; }

        public List<SearchResultEntity> Genres { get; set; }

        public List<SearchResultEntity> UserPlaylists { get; set; }

        public List<SearchResultEntity> RadioStations { get; set; } 
    }
}
