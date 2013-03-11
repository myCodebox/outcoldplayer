// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using SQLite;

    [Table("Album")]
    public class AlbumEntity
    {
        [PrimaryKey, AutoIncrement]
        public int AlbumId { get; set; }

        public string AlbumNorm { get; set; }

        public int ArtistId { get; set; }

        public string Title { get; set; }
    }
}
