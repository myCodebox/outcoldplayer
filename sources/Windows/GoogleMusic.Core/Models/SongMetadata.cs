// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public class SongMetadata
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public TimeSpan Duration { get; set; }

        public string Artist { get; set; }

        public string Composer { get; set; }

        public string Album { get; set; }

        public string AlbumArtist { get; set; }

        public ushort PlayCount { get; set; }

        public byte Rating { get; set; }

        public ushort Disc { get; set; }

        public ushort TotalDiscs { get; set; }

        public ushort Track { get; set; }

        public ushort TotalTracks { get; set; }

        public ushort Year { get; set; }

        public string Genre { get; set; }

        public Uri AlbumArtUrl { get; set; }

        public DateTime LastPlayed { get; set; }

        public DateTime CreationDate { get; set; }

        public string Comment { get; set; }

        public StreamType StreamType { get; set; }
    }
}