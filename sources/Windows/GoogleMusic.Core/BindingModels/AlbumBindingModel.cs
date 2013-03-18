// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;
    using System.Linq;

    public class AlbumBindingModel : PlaylistBaseBindingModel
    {
        public AlbumBindingModel(List<SongBindingModel> songs)
            : base(null, songs)
        {
            var song = songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Metadata.AlbumArtist))
                   ?? songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Metadata.Artist.Title));
            if (song != null)
            {
                this.Artist = string.IsNullOrWhiteSpace(song.Metadata.AlbumArtist) ? song.Metadata.Artist.Title : song.Metadata.AlbumArtist;
            }

            song = songs.FirstOrDefault(x => x.Metadata.Year > 0);
            if (song != null)
            {
                this.Year = song.Metadata.Year.Value;
            }

            song = songs.FirstOrDefault(x => !string.IsNullOrEmpty(x.Metadata.Album.Title));
            if (song != null)
            {
                this.Title = song.Metadata.Album.Title;
            }

            song = songs.FirstOrDefault(x => !string.IsNullOrEmpty(x.Metadata.Genre.Title));
            if (song != null)
            {
                this.Genre = song.Metadata.Genre.Title;
            }
        }

        public string Artist { get; private set; }

        public int Year { get; private set; }

        public string Genre { get; private set; }
    }
}