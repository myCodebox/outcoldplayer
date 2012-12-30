// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class Artist : Playlist
    {
        public Artist(List<Song> songs)
            : base(null, songs)
        {
            var song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.AlbumArtist))
                   ?? songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.Artist));
            if (song != null)
            {
                this.Title = string.IsNullOrWhiteSpace(song.GoogleMusicMetadata.AlbumArtist) ? song.GoogleMusicMetadata.Artist : song.GoogleMusicMetadata.AlbumArtist;
            }
        }
    }
}