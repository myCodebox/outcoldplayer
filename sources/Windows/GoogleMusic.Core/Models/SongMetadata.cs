// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public class SongMetadata
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public TimeSpan Duration { get; set; }

        public string Artist { get; set; }

        public string Composer { get; set; }

        public string Album { get; set; }

        public string AlbumArtist { get; set; }

        public int PlayCount { get; set; }

        public byte Rating { get; set; }

        public byte Disc { get; set; }

        public byte TotalDiscs { get; set; }

        public short Track { get; set; }

        public short TotalTracks { get; set; }

        public short Year { get; set; }

        public string Genre { get; set; }

        public Uri AlbumArtUrl { get; set; }

        public DateTime LastPlayed { get; set; }

        public DateTime CreationDate { get; set; }

        public string Comment { get; set; }

        public StreamType StreamType { get; set; }
    }
}