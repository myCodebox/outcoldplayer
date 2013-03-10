// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using SQLite;

    [Table("UserPlaylist")]
    public class UserPlaylistEntity
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Title { get; set; }
    }
}
