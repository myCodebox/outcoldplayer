// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Genre : Playlist
    {
        public Genre(string name, List<Song> songs)
            : base(
                name,
                songs.OrderBy(s => s.GoogleMusicMetadata.ArtistNorm)
                     .ThenBy(s => s.GoogleMusicMetadata.AlbumNorm)
                     .ThenBy(s => Math.Max(s.GoogleMusicMetadata.Disc, 1))
                     .ThenBy(s => s.GoogleMusicMetadata.Track)
                     .ToList())
        {
        }
    }
}