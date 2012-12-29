// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class Artist : Playlist
    {
        public Artist(List<GoogleMusicSong> songs)
            : base(null, songs)
        {
            var song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.AlbumArtist))
                   ?? songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Artist));
            if (song != null)
            {
                this.Title = string.IsNullOrWhiteSpace(song.AlbumArtist) ? song.Artist : song.AlbumArtist;
            }
        }
    }
}