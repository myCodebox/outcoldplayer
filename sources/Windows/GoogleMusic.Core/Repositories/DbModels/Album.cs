// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories.DbModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

    using SQLite;

    [Table("Album")]
    public class Album : IPlaylist
    {
        [PrimaryKey, AutoIncrement, Column("AlbumId")]
        public int Id { get; set; }

        [Ignore]
        public PlaylistType PlaylistType
        {
            get
            {
                return PlaylistType.Album;
            }
        }

        public string Title { get; set; }

        [Indexed]
        public string TitleNorm { get; set; }
        
        public int ArtistId { get; set; }

        public int SongsCount { get; set; }

        public ushort? Year { get; set; }

        public TimeSpan Duration { get; set; }

        public Uri ArtUrl { get; set; }

        [Indexed]
        public DateTime LastPlayed { get; set; }

        [Reference]
        public Artist Artist { get; set; }
    }
}
