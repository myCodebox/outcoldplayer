// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public class MusicPlaylist : Playlist
    {
        public MusicPlaylist(string id, string name, List<Song> songs, List<string> entrieIds)
            : base(name, songs)
        {
            Debug.Assert(songs.Count == entrieIds.Count, "songs.Count == entrieIds.Count");

            this.Id = id;
            this.EntriesIds = entrieIds;
        }

        public string Id { get; set; }

        public List<string> EntriesIds { get; set; }
    }
}