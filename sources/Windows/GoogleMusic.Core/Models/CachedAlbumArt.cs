// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    using SQLite;

    [Table("CachedAlbumArt")]
    public class CachedAlbumArt
    {
        [PrimaryKey]
        public Uri AlbumArtUrl { get; set; }

        public string Path { get; set; }
    }
}