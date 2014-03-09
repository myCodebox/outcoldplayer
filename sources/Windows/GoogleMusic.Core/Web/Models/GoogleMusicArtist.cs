﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    using Newtonsoft.Json;

    public class GoogleMusicArtist
    {
        public string Kind { get; set; }

        public string ArtistId { get; set; }

        public string Name { get; set; }

        public string ArtistArtRef { get; set; }

        public string ArtistBio { get; set; }

        public int? Total_Albums { get; set; }

        public GoogleMusicAlbum[] Albums { get; set; }

        public GoogleMusicSong[] TopTracks { get; set; }

        [JsonProperty("related_artists")]
        public GoogleMusicArtist[] RelatedArtists { get; set; }
    }
}
