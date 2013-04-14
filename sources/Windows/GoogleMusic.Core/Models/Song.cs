// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    using SQLite;

    [Table("Song")]
    public class Song
    {
        [PrimaryKey, AutoIncrement]
        public int SongId { get; set; }

        public string ProviderSongId { get; set; }

        public string Title { get; set; }

        [Indexed]
        public string TitleNorm { get; set; }

        public TimeSpan Duration { get; set; }

        public string Composer { get; set; }

        public ushort PlayCount { get; set; }

        public byte Rating { get; set; }

        public ushort? Disc { get; set; }
                     
        public ushort? TotalDiscs { get; set; }
                     
        public ushort? Track { get; set; }
                     
        public ushort? TotalTracks { get; set; }
                     
        public ushort? Year { get; set; }

        public Uri AlbumArtUrl { get; set; }

        [Indexed]
        public DateTime LastPlayed { get; set; }

        [Indexed]
        public DateTime CreationDate { get; set; }

        public string Comment { get; set; }

        public ushort Bitrate { get; set; }

        public StreamType StreamType { get; set; }
       
        [Reference]
        public UserPlaylistEntry UserPlaylistEntry { get; set; }

        public string AlbumArtistTitle { get; set; }

        [Indexed]
        public string AlbumArtistTitleNorm { get; set; }

        public string ArtistTitle { get; set; }

        [Indexed]
        public string ArtistTitleNorm { get; set; }

        public string AlbumTitle { get; set; }

        [Indexed]
        public string AlbumTitleNorm { get; set; }

        public string GenreTitle { get; set; }

        [Indexed]
        public string GenreTitleNorm { get; set; }
    }
}