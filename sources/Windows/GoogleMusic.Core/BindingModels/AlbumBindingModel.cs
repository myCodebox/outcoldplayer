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
                   ?? songs.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Metadata.Artist));
            if (song != null)
            {
                this.Artist = string.IsNullOrWhiteSpace(song.Metadata.AlbumArtist) ? song.Metadata.Artist : song.Metadata.AlbumArtist;
            }

            song = songs.FirstOrDefault(x => x.Metadata.Year > 0);
            if (song != null)
            {
                this.Year = song.Metadata.Year;
            }

            song = songs.FirstOrDefault(x => !string.IsNullOrEmpty(x.Metadata.Album));
            if (song != null)
            {
                this.Title = song.Metadata.Album;
            }

            song = songs.FirstOrDefault(x => !string.IsNullOrEmpty(x.Metadata.Genre));
            if (song != null)
            {
                this.Genre = song.Metadata.Genre;
            }
        }

        public string Artist { get; private set; }

        public int Year { get; private set; }

        public string Genre { get; private set; }
    }
}