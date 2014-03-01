// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Synchronization
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public static class GoogleMusicRadioEx
    {
        public static Radio ToRadio(this GoogleMusicRadio googleMusicRadio)
        {
            var radio = new Radio();

            Mapper(googleMusicRadio, radio);

            return radio;
        }

        public static void Mapper(GoogleMusicRadio googleMusicRadio, Radio radio)
        {
            radio.RadioId = googleMusicRadio.Id;
            radio.Title = googleMusicRadio.Name;
            radio.TitleNorm = googleMusicRadio.Name.Normalize();
            radio.LastModified =
                DateTimeExtensions.FromUnixFileTime(googleMusicRadio.LastModifiedTimestamp / 1000);
            radio.Recent = DateTimeExtensions.FromUnixFileTime(googleMusicRadio.RecentTimestamp / 1000);
            radio.ClientId = googleMusicRadio.ClientId;
            radio.ArtUrl = string.IsNullOrEmpty(googleMusicRadio.ImageUrl) ? null : new Uri(googleMusicRadio.ImageUrl);
            if (googleMusicRadio.Seed != null)
            {
                radio.SeedType = googleMusicRadio.Seed.SeedType;
                radio.TrackLockerId = googleMusicRadio.Seed.TrackLockerId;
                radio.SongId = googleMusicRadio.Seed.TrackId;
                radio.GoogleArtistId = googleMusicRadio.Seed.ArtistId;
                radio.GoogleAlbumId = googleMusicRadio.Seed.AlbumId;
            }
        }
    }
}
