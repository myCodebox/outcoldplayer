// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;
    using System.Linq;

    public class SystemPlaylist : Playlist
    {
        public SystemPlaylist(string name, SystemPlaylistType type, IEnumerable<Song> songs)
            : base(
            name,
            songs.ToList())
        {
            this.Type = type;
            this.AlbumArtUrl = null;
        }

        public enum SystemPlaylistType
        {
            AllSongs,

            HighlyRated,

            LastAdded
        }

        public SystemPlaylistType Type { get; set; }
    }
}