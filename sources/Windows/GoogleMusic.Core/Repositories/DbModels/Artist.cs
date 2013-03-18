// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories.DbModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

    using SQLite;

    [Table("Artist")]
    public class Artist : IPlaylist
    {
        [PrimaryKey, AutoIncrement, Column("ArtistId")]
        public int Id { get; set; }

        [Ignore]
        public PlaylistType PlaylistType
        {
            get
            {
                return PlaylistType.Artist;
            }
        }

        public string Title { get; set; }

        [Indexed]
        public string TitleNorm { get; set; }

        public int AlbumsCount { get; set; }

        public int SongsCount { get; set; }

        public TimeSpan Duration { get; set; }

        public Uri ArtUrl { get; set; }

        [Indexed]
        public DateTime LastPlayed { get; set; }
    }
}
