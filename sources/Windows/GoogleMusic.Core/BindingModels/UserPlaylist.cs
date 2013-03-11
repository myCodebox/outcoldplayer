// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.Models;

    public class UserPlaylist : Playlist
    {
        public UserPlaylist(UserPlaylistEntity metadata, string name, List<Song> songs, List<string> entrieIds)
            : base(name, songs)
        {
            Debug.Assert(songs.Count == entrieIds.Count, "songs.Count == entrieIds.Count");

            this.Metadata = metadata;
            this.EntriesIds = entrieIds;
        }

        public UserPlaylistEntity Metadata { get; set; }

        public List<string> EntriesIds { get; set; }
    }
}