// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    using SQLite;

    [Table("Radio")]
    public class Radio : IPlaylist
    {
        [PrimaryKey, Column("RadioId")]
        public string RadioId { get; set; }

        [Ignore]
        public string Id
        {
            get
            {
                return this.RadioId;
            }
        }

        [Ignore]
        public PlaylistType PlaylistType
        {
            get
            {
                return PlaylistType.Radio;
            }
        }

        public string Title { get; set; }

        [Indexed]
        public string TitleNorm { get; set; }

        [Ignore]
        public int SongsCount { get; set; }

        [Ignore]
        public int OfflineSongsCount { get; set; }

        [Ignore]
        public TimeSpan Duration { get; set; }

        [Ignore]
        public TimeSpan OfflineDuration { get; set; }

        public Uri ArtUrl { get; set; }

        [Indexed]
        public DateTime Recent { get; set; }

        public string ClientId { get; set; }

        public DateTime LastModified { get; set; }

        public int SeedType { get; set; }

        public string SongId { get; set; }

        public string TrackLockerId { get; set; }

        public string GoogleArtistId { get; set; }

        public string GoogleAlbumId { get; set; }
    }
}
