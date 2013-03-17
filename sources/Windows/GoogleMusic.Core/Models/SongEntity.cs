// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    using SQLite;

    [Table("Song")]
    public class SongEntity
    {
        [PrimaryKey, AutoIncrement]
        public int SongId { get; set; }

        public string ProviderSongId { get; set; }

        public string Title { get; set; }

        [Indexed]
        public string TitleNorm { get; set; }

        public TimeSpan Duration { get; set; }

        public string Artist { get; set; }

        [Indexed]
        [Indexed(Name = "Song_AlbumNorm_ArtistNorm")]
        public string ArtistNorm { get; set; }

        public string Composer { get; set; }

        public string Album { get; set; }

        [Indexed]
        [Indexed(Name = "Song_AlbumNorm_ArtistNorm")]
        public string AlbumNorm { get; set; }

        public string AlbumArtist { get; set; }

        [Indexed]
        public string AlbumArtistNorm { get; set; }

        public ushort PlayCount { get; set; }

        public byte Rating { get; set; }

        public ushort Disc { get; set; }

        public ushort TotalDiscs { get; set; }

        public ushort Track { get; set; }

        public ushort TotalTracks { get; set; }

        public ushort Year { get; set; }

        public string Genre { get; set; }

        [Indexed]
        public string GenreNorm { get; set; }

        public Uri AlbumArtUrl { get; set; }

        [Indexed]
        public DateTime LastPlayed { get; set; }

        [Indexed]
        public DateTime CreationDate { get; set; }

        public string Comment { get; set; }

        public ushort Bitrate { get; set; }

        public StreamType StreamType { get; set; }
    }
}