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
            return new Song()
            {
                AlbumArtistTitle = googleMusicSong.AlbumArtist.Trim(),
                AlbumArtistTitleNorm = googleMusicSong.AlbumArtist.Trim().Normalize(),
                ArtistTitle = googleMusicSong.Artist.Trim(),
                ArtistTitleNorm = googleMusicSong.Artist.Trim().Normalize(),
                AlbumTitle = googleMusicSong.Album.Trim(),
                AlbumTitleNorm = googleMusicSong.Album.Trim().Normalize(),
                GenreTitle = googleMusicSong.Genre.Trim(),
                GenreTitleNorm = googleMusicSong.Genre.Trim().Normalize(),
                AlbumArtUrl = string.IsNullOrEmpty(googleMusicSong.AlbumArtUrl) ? null : new Uri("http:" + googleMusicSong.AlbumArtUrl),
                Composer = googleMusicSong.Composer,
                Disc = googleMusicSong.Disc,
                TotalDiscs = googleMusicSong.TotalDiscs,
                Duration = TimeSpan.FromMilliseconds(googleMusicSong.DurationMillis),
                ProviderSongId = googleMusicSong.Id,
                LastPlayed = DateTimeExtensions.FromUnixFileTime(googleMusicSong.LastPlayed / 1000),
                CreationDate = DateTimeExtensions.FromUnixFileTime(googleMusicSong.CreationDate / 1000),
                PlayCount = googleMusicSong.PlayCount,
                Rating = googleMusicSong.Rating,
                Title = googleMusicSong.Title.Trim(),
                TitleNorm = googleMusicSong.Title.Trim().Normalize(),
                Track = googleMusicSong.Track,
                TotalTracks = googleMusicSong.TotalTracks,
                Year = googleMusicSong.Year,
                Comment = googleMusicSong.Comment,
                Bitrate = googleMusicSong.Bitrate,
                StreamType = StreamType.GoogleMusic
            };
        }
    }
}
