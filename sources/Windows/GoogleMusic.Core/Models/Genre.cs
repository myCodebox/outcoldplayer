// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class Genre : Playlist
    {
        public Genre(string name, List<Song> songs)
            : base(name, songs.OrderBy(s => s.GoogleMusicMetadata.ArtistNorm).OrderBy(s => s.GoogleMusicMetadata.AlbumNorm).ThenBy(s => s.GoogleMusicMetadata.Disc).ThenBy(s => s.GoogleMusicMetadata.Track).ToList())
        {
        }
    }
}