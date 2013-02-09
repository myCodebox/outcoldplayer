// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SystemPlaylist : Playlist
    {
        public SystemPlaylist(string name, SystemPlaylistType type, IEnumerable<Song> songs)
            : base(
            name, 
            songs.OrderBy(s => s.Metadata.Artist, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(s => s.Metadata.Album, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(s => Math.Max(s.Metadata.Disc, (byte)1))
                .ThenBy(s => s.Metadata.Track).ToList())
        {
            this.Type = type;
            this.AlbumArtUrl = null;
        }

        public enum SystemPlaylistType
        {
            AllSongs,

            HighlyRated
        }

        public SystemPlaylistType Type { get; set; }
    }
}