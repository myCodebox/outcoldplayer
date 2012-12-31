// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public class MusicPlaylist : Playlist
    {
        public MusicPlaylist(string name, List<Song> songs, List<string> entrieIds)
            : base(name, songs)
        {
            Debug.Assert(songs.Count == entrieIds.Count, "songs.Count == entrieIds.Count");

            this.EntriesIds = entrieIds;
        }

        public List<string> EntriesIds { get; set; }
    }
}