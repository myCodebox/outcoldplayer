// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class Album : Playlist
    {
        public Album(List<Song> songs)
            : base(null, songs.OrderBy(s => s.GoogleMusicMetadata.Disc).ThenBy(s => s.GoogleMusicMetadata.Track).ToList())
        {
            var song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.AlbumArtist))
                   ?? songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.Artist));
            if (song != null)
            {
                this.Artist = string.IsNullOrWhiteSpace(song.GoogleMusicMetadata.AlbumArtist) ? song.GoogleMusicMetadata.Artist : song.GoogleMusicMetadata.AlbumArtist;
            }

            song = songs.FirstOrDefault(x => x.GoogleMusicMetadata.Year > 0);
            if (song != null)
            {
                this.Year = song.GoogleMusicMetadata.Year;
            }

            song = songs.FirstOrDefault(x => !string.IsNullOrEmpty(x.GoogleMusicMetadata.Album));
            if (song != null)
            {
                this.Title = song.GoogleMusicMetadata.Album;
            }

            song = songs.FirstOrDefault(x => !string.IsNullOrEmpty(x.GoogleMusicMetadata.Genre));
            if (song != null)
            {
                this.Genre = song.GoogleMusicMetadata.Genre;
            }
        }

        public string Artist { get; private set; }

        public int Year { get; private set; }

        public string Genre { get; private set; }
    }
}