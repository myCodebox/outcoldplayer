// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    using SQLite;

    [Table("Genre")]
    public class Genre : IPlaylist
    {
        [PrimaryKey, AutoIncrement, Column("GenreId")]
        public int Id { get; set; }

        [Ignore]
        public PlaylistType PlaylistType
        {
            get
            {
                return PlaylistType.Genre;
            }
        }

        public string Title { get; set; }

        [Indexed]
        public string TitleNorm { get; set; }

        public int SongsCount { get; set; }

        public int OfflineSongsCount { get; set; }

        public TimeSpan Duration { get; set; }

        public TimeSpan OfflineDuration { get; set; }

        public Uri ArtUrl { get; set; }

        [Indexed]
        public DateTime LastPlayed { get; set; }
    }
}
