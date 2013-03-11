// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using SQLite;

    [Table("AlbumEntry")]
    public class AlbumEntryEntity
    {
        [PrimaryKey, AutoIncrement]
        public int AlbumEntryId { get; set; }

        public int SongId { get; set; }

        public int AlbumId { get; set; }
    }
}
