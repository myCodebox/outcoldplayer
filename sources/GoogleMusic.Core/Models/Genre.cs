// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;

    public class Genre : Playlist
    {
        public Genre(string name, List<Song> songs)
            : base(name, songs)
        {
        }
    }
}