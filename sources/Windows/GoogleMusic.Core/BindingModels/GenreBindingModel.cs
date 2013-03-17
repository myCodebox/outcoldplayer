// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class GenreBindingModel : PlaylistBaseBindingModel
    {
        public GenreBindingModel(string name, List<SongBindingModel> songs)
            : base(
                name,
                songs.OrderBy(s => s.Metadata.Artist, StringComparer.CurrentCultureIgnoreCase)
                     .ThenBy(s => s.Metadata.Album, StringComparer.CurrentCultureIgnoreCase)
                     .ThenBy(s => Math.Max(s.Metadata.Disc, (byte)1))
                     .ThenBy(s => s.Metadata.Track)
                     .ToList())
        {
        }
    }
}