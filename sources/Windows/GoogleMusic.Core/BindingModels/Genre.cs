// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Genre : Playlist
    {
        public Genre(string name, List<Song> songs)
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