// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class Album : Playlist
    {
        public Album(List<GoogleMusicSong> songs)
            : base(null, songs)
        {
            var song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.AlbumArtist))
                   ?? songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Artist));
            if (song != null)
            {
                this.Artist = string.IsNullOrWhiteSpace(song.AlbumArtist) ? song.Artist : song.AlbumArtist;
            }

            song = songs.FirstOrDefault(x => x.Year > 0);
            if (song != null)
            {
                this.Year = song.Year;
            }

            song = songs.FirstOrDefault(x => !string.IsNullOrEmpty(x.Album));
            if (song != null)
            {
                this.Title = song.Album;
            }

            song = songs.FirstOrDefault(x => !string.IsNullOrEmpty(x.Genre));
            if (song != null)
            {
                this.Genre = song.Genre;
            }
        }

        public string Artist { get; private set; }

        public int Year { get; private set; }

        public string Genre { get; private set; }
    }
}