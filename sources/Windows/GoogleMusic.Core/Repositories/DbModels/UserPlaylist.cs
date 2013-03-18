// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories.DbModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

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

        public TimeSpan Duration { get; set; }

        public Uri ArtUrl { get; set; }

        [Indexed]
        public DateTime LastPlayed { get; set; }
    }
}
