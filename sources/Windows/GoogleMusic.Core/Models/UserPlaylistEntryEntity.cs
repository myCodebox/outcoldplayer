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

        public int PlaylistId { get; set; }

        public int SongId { get; set; }

        public int PlaylistOrder { get; set; }

        public string ProviderEntryId { get; set; }
    }
}
