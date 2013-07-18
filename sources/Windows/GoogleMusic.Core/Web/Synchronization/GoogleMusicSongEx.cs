// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Synchronization
{
    using System;
    using System.Globalization;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public static class GoogleMusicSongEx
    {
        public static Song ToSong(this GoogleMusicSong googleMusicSong)
        {
            var song = new Song();

            Mapper(googleMusicSong, song);

            return song;
        }

        public static void Mapper(GoogleMusicSong googleMusicSong, Song song)
        {
            song.AlbumArtistTitle = (googleMusicSong.AlbumArtist ?? string.Empty).Trim();
            song.AlbumArtistTitleNorm = (googleMusicSong.AlbumArtist ?? string.Empty).Trim().Normalize();
            song.ArtistTitle = (googleMusicSong.Artist ?? string.Empty).Trim();
            song.ArtistTitleNorm = (googleMusicSong.Artist ?? string.Empty).Trim().Normalize();
            song.AlbumTitle = (googleMusicSong.Album ?? string.Empty).Trim();
            song.AlbumTitleNorm = (googleMusicSong.Album ?? string.Empty).Trim().Normalize();
            song.GenreTitle = (googleMusicSong.Genre ?? string.Empty).Trim();
            song.GenreTitleNorm = (googleMusicSong.Genre ?? string.Empty).Trim().Normalize();
            string imageBaseUrlBase = googleMusicSong.AlbumArtUrl ?? googleMusicSong.ImageBaseUrl;
            song.AlbumArtUrl = string.IsNullOrEmpty(imageBaseUrlBase)
                ? null
                : new Uri("http:" + imageBaseUrlBase);
            song.Composer = googleMusicSong.Composer;
            song.Disc = googleMusicSong.Disc;
            song.TotalDiscs = googleMusicSong.TotalDiscs;
            song.Duration = TimeSpan.FromMilliseconds(googleMusicSong.DurationMillis);
            song.ProviderSongId = googleMusicSong.Id;
            song.LastPlayed = DateTimeExtensions.FromUnixFileTime(googleMusicSong.LastPlayed / 1000);
            song.CreationDate = DateTimeExtensions.FromUnixFileTime(googleMusicSong.CreationDate / 1000);
            song.PlayCount = googleMusicSong.PlayCount;
            song.Rating = (byte)(googleMusicSong.Rating < 0 ? 0 : googleMusicSong.Rating);
            song.Title = (googleMusicSong.Title ?? string.Empty).Trim();
            song.TitleNorm = (googleMusicSong.Title ?? string.Empty).Trim().Normalize();
            song.Track = googleMusicSong.Track;
            song.TotalTracks = googleMusicSong.TotalTracks;
            song.Year = googleMusicSong.Year;
            song.Comment = googleMusicSong.Comment;
            song.Bitrate = googleMusicSong.Bitrate;

            int type;
            if (!int.TryParse(googleMusicSong.Type, out type))
            {
                if (string.Equals(googleMusicSong.Type, "EPHEMERAL_SUBSCRIPTION", StringComparison.OrdinalIgnoreCase))
                {
                    type = 1;
                }
                else
                {
                    type = 2;
                }
            }

            song.StreamType = (StreamType)type;
            song.StoreId = googleMusicSong.StoreId;
            song.IsLibrary = true;
        }

        public static bool IsVisualMatch(GoogleMusicSong googleMusicSong, Song song)
        {
            return string.Equals(song.AlbumArtistTitleNorm, googleMusicSong.AlbumArtist.Trim().Normalize(), StringComparison.CurrentCulture)
            && string.Equals(song.ArtistTitleNorm, googleMusicSong.Artist.Trim().Normalize(), StringComparison.CurrentCulture)
            && string.Equals(song.AlbumTitleNorm, googleMusicSong.Album.Trim().Normalize(), StringComparison.CurrentCulture)
            && string.Equals(song.GenreTitleNorm, googleMusicSong.Genre.Trim().Normalize(), StringComparison.CurrentCulture)
            && (song.AlbumArtUrl == (string.IsNullOrEmpty(googleMusicSong.AlbumArtUrl) ? null : new Uri("http:" + googleMusicSong.AlbumArtUrl))) 
            && (song.Rating == googleMusicSong.Rating)
            && string.Equals(song.TitleNorm, googleMusicSong.Title.Trim().Normalize(), StringComparison.CurrentCulture)
            && (song.Track == googleMusicSong.Track)
            && (song.Year == googleMusicSong.Year);
        }
    }
}
