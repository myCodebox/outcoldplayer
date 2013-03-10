// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Artist : Playlist
    {
        public Artist(List<Song> songs)
            : this(songs, false)
        {
            var song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Metadata.AlbumArtist))
                   ?? songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Metadata.Artist));

            if (song != null)
            {
                this.Title = string.IsNullOrWhiteSpace(song.Metadata.AlbumArtist) ? song.Metadata.Artist : song.Metadata.AlbumArtist;
            }
        }

        public Artist(List<Song> songs, bool useArtist)
            : base(
                null, 
                songs.OrderBy(s => s.Metadata.Album, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(s => Math.Max(s.Metadata.Disc, (byte)1))
                    .ThenBy(s => s.Metadata.Track).ToList())
        {
            Song song;

            if (useArtist)
            {
                song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Metadata.Artist));

                if (song != null)
                {
                    this.Title = song.Metadata.Artist;
                }
            }
            else
            {
                song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Metadata.AlbumArtist))
                           ?? songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Metadata.Artist));

                if (song != null)
                {
                    this.Title = string.IsNullOrWhiteSpace(song.Metadata.AlbumArtist) ? song.Metadata.Artist : song.Metadata.AlbumArtist;
                }
            }
        }
    }
}