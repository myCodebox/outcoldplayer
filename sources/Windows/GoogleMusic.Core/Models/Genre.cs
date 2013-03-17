// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public class Genre
    {
        public string Title { get; set; }

        public string TitleNorm { get; set; }

        public int SongsCount { get; set; }

        public TimeSpan Duration { get; set; }

        public Uri AlbumArtUrl { get; set; }

        public DateTime LastPlayed { get; set; }
    }
}
