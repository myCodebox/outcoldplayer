// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    using SQLite;

    [Table("UserPlaylist")]
    public class UserPlaylist : IPlaylist
    {
        [PrimaryKey, AutoIncrement, Column("PlaylistId")]
        public int Id { get; set; }

        [Ignore]
        public PlaylistType PlaylistType
        {
            get
            {
                return PlaylistType.UserPlaylist;
            }
        }

        public string ProviderPlaylistId { get; set; }

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
