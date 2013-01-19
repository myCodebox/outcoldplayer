// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Artist : Playlist
    {
        public Artist(List<Song> songs)
            : this(songs, false)
        {
            var song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.AlbumArtist))
                   ?? songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.Artist));

            if (song != null)
            {
                this.Title = string.IsNullOrWhiteSpace(song.GoogleMusicMetadata.AlbumArtist) ? song.GoogleMusicMetadata.Artist : song.GoogleMusicMetadata.AlbumArtist;
            }
        }

        public Artist(List<Song> songs, bool useArtist)
            : base(null, songs.OrderBy(s => s.GoogleMusicMetadata.AlbumNorm).ThenBy(s => Math.Max(s.GoogleMusicMetadata.Disc, 1)).ThenBy(s => s.GoogleMusicMetadata.Track).ToList())
        {
            Song song;

            if (useArtist)
            {
                song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.Artist));

                if (song != null)
                {
                    this.Title = song.GoogleMusicMetadata.Artist;
                }
            }
            else
            {
                song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.AlbumArtist))
                           ?? songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.Artist));

                if (song != null)
                {
                    this.Title = string.IsNullOrWhiteSpace(song.GoogleMusicMetadata.AlbumArtist) ? song.GoogleMusicMetadata.Artist : song.GoogleMusicMetadata.AlbumArtist;
                }
            }
        }
    }
}