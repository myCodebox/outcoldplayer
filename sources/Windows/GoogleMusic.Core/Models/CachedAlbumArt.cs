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

        [PrimaryKey]
        public uint Size { get; set; }

        public string FileName { get; set; }
    }
}