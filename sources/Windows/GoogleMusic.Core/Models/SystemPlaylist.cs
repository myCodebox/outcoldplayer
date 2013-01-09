// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class SystemPlaylist : Playlist
    {
        public SystemPlaylist(string name, SystemPlaylistType type, IEnumerable<Song> songs)
            : base(name, songs.OrderBy(s => s.GoogleMusicMetadata.ArtistNorm).OrderBy(s => s.GoogleMusicMetadata.AlbumNorm).ThenBy(s => s.GoogleMusicMetadata.Disc).ThenBy(s => s.GoogleMusicMetadata.Track).ToList())
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