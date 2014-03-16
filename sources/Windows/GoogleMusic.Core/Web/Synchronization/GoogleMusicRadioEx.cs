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
            if (googleMusicRadio.ImageUrls != null && googleMusicRadio.ImageUrls.Length > 1)
            {
                if (googleMusicRadio.ImageUrls.Length >= 4)
                {
                    radio.ArtUrl3 = new Uri(googleMusicRadio.ImageUrls[3].Url);
                }

                if (googleMusicRadio.ImageUrls.Length >= 3)
                {
                    radio.ArtUrl2 = new Uri(googleMusicRadio.ImageUrls[2].Url);
                }

                if (googleMusicRadio.ImageUrls.Length >= 2)
                {
                    radio.ArtUrl1 = new Uri(googleMusicRadio.ImageUrls[1].Url);
                }

                if (googleMusicRadio.ImageUrls.Length >= 1)
                {
                    radio.ArtUrl = new Uri(googleMusicRadio.ImageUrls[0].Url);
                }
            }
            if (googleMusicRadio.Seed != null)
            {
                radio.SeedType = googleMusicRadio.Seed.SeedType;
                radio.TrackLockerId = googleMusicRadio.Seed.TrackLockerId;
                radio.SongId = googleMusicRadio.Seed.TrackId;
                radio.GoogleArtistId = googleMusicRadio.Seed.ArtistId;
                radio.GoogleAlbumId = googleMusicRadio.Seed.AlbumId;
                radio.GoogleGenreId = googleMusicRadio.Seed.GenreId;
            }
        }
    }
}
