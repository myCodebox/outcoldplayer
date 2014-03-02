// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Synchronization
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public static class GoogleMusicSongEx
    {
        public static Song ToSong(this GoogleMusicSong googleMusicSong)
        {
            var song = new Song { IsLibrary = true };

            Mapper(googleMusicSong, song);

            return song;
        }

        public static void Mapper(GoogleMusicSong googleMusicSong, Song song)
        {
            song.ClientId = googleMusicSong.ClientId;
            song.AlbumArtistTitle = (googleMusicSong.AlbumArtist ?? string.Empty).Trim();
            song.AlbumArtistTitleNorm = (googleMusicSong.AlbumArtist ?? string.Empty).Trim().Normalize();
            song.ArtistTitle = (googleMusicSong.Artist ?? string.Empty).Trim();
            song.ArtistTitleNorm = (googleMusicSong.Artist ?? string.Empty).Trim().Normalize();
            song.AlbumTitle = (googleMusicSong.Album ?? string.Empty).Trim();
            song.AlbumTitleNorm = (googleMusicSong.Album ?? string.Empty).Trim().Normalize();
            song.GenreTitle = (googleMusicSong.Genre ?? string.Empty).Trim();
            song.GenreTitleNorm = (googleMusicSong.Genre ?? string.Empty).Trim().Normalize();
            song.ArtistArtUrl = googleMusicSong.ArtistArtRef == null || googleMusicSong.ArtistArtRef.Length == 0 ? null : new Uri(googleMusicSong.ArtistArtRef[0].Url);
            song.AlbumArtUrl = googleMusicSong.AlbumArtRef == null || googleMusicSong.AlbumArtRef.Length == 0 ? null : new Uri(googleMusicSong.AlbumArtRef[0].Url);
            song.Composer = googleMusicSong.Composer;
            song.Disc = googleMusicSong.DiscNumber;
            song.TotalDiscs = googleMusicSong.TotalDiscCount;
            song.Duration = TimeSpan.FromMilliseconds(googleMusicSong.DurationMillis);
            song.SongId = string.IsNullOrEmpty(googleMusicSong.Id) ? googleMusicSong.StoreId : googleMusicSong.Id;
            song.ServerRecent = DateTimeExtensions.FromUnixFileTime(googleMusicSong.RecentTimestamp / 1000);
            song.CreationDate = DateTimeExtensions.FromUnixFileTime(googleMusicSong.CreationTimestamp / 1000);
            song.LastModified = DateTimeExtensions.FromUnixFileTime(googleMusicSong.LastModifiedTimestamp / 1000);
            song.BeatsPerMinute = googleMusicSong.BeatsPerMinute;
            song.EstimatedSize = googleMusicSong.EstimatedSize;
            song.ServerPlayCount = googleMusicSong.PlayCount;
            song.Rating = (byte)(googleMusicSong.Rating < 0 ? 0 : googleMusicSong.Rating);
            song.Title = (googleMusicSong.Title ?? string.Empty).Trim();
            song.TitleNorm = (googleMusicSong.Title ?? string.Empty).Trim().Normalize();
            song.Track = googleMusicSong.TrackNumber;
            song.TotalTracks = googleMusicSong.TotalTrackCount;
            song.Year = googleMusicSong.Year;
            song.Comment = googleMusicSong.Comment;
            song.TrackType = (StreamType)googleMusicSong.TrackType;
            song.ContentType = googleMusicSong.ContentType;
            song.TrackAvailableForSubscription = googleMusicSong.TrackAvailableForSubscription;
            song.TrackAvailableForPurchase = googleMusicSong.TrackAvailableForPurchase;
            song.AlbumAvailableForPurchase = googleMusicSong.AlbumAvailableForPurchase;
            song.StoreId = googleMusicSong.StoreId;
            song.GoogleAlbumId = googleMusicSong.AlbumId;
            // TODO: We do not support songs with multiple artists
            song.GoogleArtistId = googleMusicSong.ArtistId != null && googleMusicSong.ArtistId.Length != 0
                ? googleMusicSong.ArtistId[0]
                : string.Empty;
            song.Nid = googleMusicSong.Nid;
            song.Recent = song.ServerRecent > song.StatsRecent ? song.ServerRecent : song.StatsRecent;
            song.PlayCount = Math.Max(song.ServerPlayCount, song.PlayCount);
        }

        public static bool IsVisualMatch(GoogleMusicSong googleMusicSong, Song song)
        {
            return string.Equals(song.AlbumArtistTitleNorm, googleMusicSong.AlbumArtist.Trim().Normalize(), StringComparison.CurrentCulture)
            && string.Equals(song.ArtistTitleNorm, googleMusicSong.Artist.Trim().Normalize(), StringComparison.CurrentCulture)
            && string.Equals(song.AlbumTitleNorm, googleMusicSong.Album.Trim().Normalize(), StringComparison.CurrentCulture)
            && string.Equals(song.GenreTitleNorm, googleMusicSong.Genre.Trim().Normalize(), StringComparison.CurrentCulture)
            && (song.Rating == googleMusicSong.Rating);
        }
    }
}
