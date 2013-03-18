// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories.DbModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

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

        public TimeSpan Duration { get; set; }

        public Uri ArtUrl { get; set; }

        [Indexed]
        public DateTime LastPlayed { get; set; }
    }
}
