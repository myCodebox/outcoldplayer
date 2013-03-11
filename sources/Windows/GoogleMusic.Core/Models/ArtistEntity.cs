// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using SQLite;

    [Table("Artist")]
    public class ArtistEntity
    {
        [PrimaryKey, AutoIncrement]
        public int AlbumId { get; set; }

        public string AlbumNorm { get; set; }

        public string Title { get; set; }
    }
}
