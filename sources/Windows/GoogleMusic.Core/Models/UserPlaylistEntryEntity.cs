// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using SQLite;

    [Table("UserPlaylistEntry")]
    public class UserPlaylistEntryEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string PlaylistId { get; set; }

        public string SongId { get; set; }

        public int PlaylistOrder { get; set; }

        public string GoogleMusicEntryId { get; set; }
    }
}
