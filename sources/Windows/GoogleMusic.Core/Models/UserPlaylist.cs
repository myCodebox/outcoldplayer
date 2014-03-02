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
        [PrimaryKey]
        public string PlaylistId { get; set; }

        [Ignore]
        public string Id
        {
            get
            {
                return this.PlaylistId;
            }
        }

        [Ignore]
        public PlaylistType PlaylistType
        {
            get
            {
                return PlaylistType.UserPlaylist;
            }
        }

        public string Title { get; set; }

        [Indexed]
        public string TitleNorm { get; set; }

        public int SongsCount { get; set; }

        public int OfflineSongsCount { get; set; }

        public TimeSpan Duration { get; set; }

        public TimeSpan OfflineDuration { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModified { get; set; }

        public DateTime RecentDate { get; set; }

        public string Type { get; set; }

        public string ShareToken { get; set; }

        public string OwnerName { get; set; }

        public string OwnerProfilePhotoUrl { get; set; }

        public bool AccessControlled { get; set; }

        public string Description { get; set; }

        public Uri ArtUrl { get; set; }

        [Indexed]
        public DateTime Recent { get; set; }

        public bool IsShared
        {
            get
            {
                return string.Equals(this.Type, "SHARED", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
