// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class Genre : Playlist
    {
        public Genre(string name, List<GoogleMusicSong> songs)
            : base(name, songs)
        {
        }
    }
}