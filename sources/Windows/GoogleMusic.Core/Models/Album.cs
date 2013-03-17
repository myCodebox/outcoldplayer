// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public class Album
    {
        public string Title { get; set; }

        public string TitleNorm { get; set; }

        public string Artist { get; set; }

        public string ArtistNorm { get; set; }

        public string Genre { get; set; }

        public string GenreNorm { get; set; }

        public int SongsCount { get; set; }

        public TimeSpan Duration { get; set; }

        public Uri AlbumArtUrl { get; set; }

        public DateTime LastPlayed { get; set; }
    }
}
