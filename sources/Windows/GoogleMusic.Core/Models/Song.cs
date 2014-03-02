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
        [PrimaryKey]
        public string SongId { get; set; }

        public string ClientId { get; set; }

        public string Title { get; set; }

        [Indexed]
        public string TitleNorm { get; set; }

        public TimeSpan Duration { get; set; }

        public string Composer { get; set; }

        public uint PlayCount { get; set; }

        public byte Rating { get; set; }

        public int? Disc { get; set; }

        public int? TotalDiscs { get; set; }

        public int? Track { get; set; }

        public int? TotalTracks { get; set; }

        public int? Year { get; set; }

        public Uri AlbumArtUrl { get; set; }

        public Uri ArtistArtUrl { get; set; }

        public DateTime ServerRecent { get; set; }

        public DateTime CreationDate { get; set; }

        public string Comment { get; set; }

        public StreamType TrackType { get; set; }

        public string GoogleArtistId { get; set; }

        public string GoogleAlbumId { get; set; }

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

        public bool IsCached { get; set; }

        public bool IsLibrary { get; set; }

        public string StoreId { get; set; }

        public DateTime LastModified { get; set; }

        public int BeatsPerMinute { get; set; }

        public string EstimatedSize { get; set; }

        public int ContentType { get; set; }

        public bool TrackAvailableForSubscription { get; set; }

        public bool TrackAvailableForPurchase { get; set; }

        public bool AlbumAvailableForPurchase { get; set; }

        public string Nid { get; set; }

        // Statistics

        [Indexed]
        public uint StatsPlayCount { get; set; }

        public DateTime StatsRecent { get; set; }

        [Indexed]
        public DateTime Recent { get; set; }

        public uint ServerPlayCount { get; set; }
    }
}