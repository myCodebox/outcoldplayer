// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Globalization;

    using SQLite;

    [Table("Album")]
    public class Album : IPlaylist
    {
        [PrimaryKey, AutoIncrement, Column("AlbumId")]
        public int AlbumId { get; set; }

        [Ignore]
        public string Id
        {
            get
            {
                return this.AlbumId.ToString(CultureInfo.InvariantCulture);
            }
        }

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
        
        [Indexed]
        public string ArtistTitleNorm { get; set; }

        [Indexed]
        public string GenreTitleNorm { get; set; }

        public int SongsCount { get; set; }

        public int OfflineSongsCount { get; set; }

        public ushort? Year { get; set; }

        public TimeSpan Duration { get; set; }

        public TimeSpan OfflineDuration { get; set; }

        public Uri ArtUrl { get; set; }

        [Ignore]
        public bool IsCollection { get; set; }

        [Indexed]
        public DateTime LastPlayed { get; set; }

        [Reference]
        public Artist Artist { get; set; }
    }
}
