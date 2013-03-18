// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ArtistBindingModel : PlaylistBaseBindingModel
    {
        public ArtistBindingModel(List<SongBindingModel> songs)
            : this(songs, false)
        {
            var song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Metadata.AlbumArtist))
                   ?? songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Metadata.Artist.Title));

            if (song != null)
            {
                this.Title = string.IsNullOrWhiteSpace(song.Metadata.AlbumArtist) ? song.Metadata.Artist.Title : song.Metadata.AlbumArtist;
            }
        }

        public ArtistBindingModel(List<SongBindingModel> songs, bool useArtist)
            : base(
                null, 
                songs.OrderBy(s => s.Metadata.Album.Title, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(s => Math.Max(s.Metadata.Disc.Value, (byte)1))
                    .ThenBy(s => s.Metadata.Track).ToList())
        {
            SongBindingModel song;

            if (useArtist)
            {
                song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Metadata.Artist.Title));

                if (song != null)
                {
                    this.Title = song.Metadata.Artist.Title;
                }
            }
            else
            {
                song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Metadata.AlbumArtist))
                           ?? songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Metadata.Artist.Title));

                if (song != null)
                {
                    this.Title = string.IsNullOrWhiteSpace(song.Metadata.AlbumArtist) ? song.Metadata.Artist.Title : song.Metadata.AlbumArtist;
                }
            }
        }
    }
}