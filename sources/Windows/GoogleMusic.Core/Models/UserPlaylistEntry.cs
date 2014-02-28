// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    using SQLite;

    [Table("UserPlaylistEntry")]
    public class UserPlaylistEntry
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string PlaylistId { get; set; }

        public string SongId { get; set; }

        public string PlaylistOrder { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModified { get; set; }

        public string CliendId { get; set; }

        public int Source { get; set; } 

        [Reference]
        public Song Song { get; set; }
    }
}
