// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public class Artist 
    {
        public string TitleNorm { get; set; }

        public string Title { get; set; }

        public int AlbumsCount { get; set; }

        public int SongsCount { get; set; }

        public TimeSpan Duration { get; set; }

        public Uri ArtistArtUrl { get; set; }

        public DateTime LastPlayed { get; set; }
    }
}
