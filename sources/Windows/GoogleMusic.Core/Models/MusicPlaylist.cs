// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class MusicPlaylist : Playlist
    {
        public MusicPlaylist(Guid id, string name, List<Song> songs, List<string> entrieIds)
            : base(name, songs)
        {
            Debug.Assert(songs.Count == entrieIds.Count, "songs.Count == entrieIds.Count");

            this.Id = id;
            this.EntriesIds = entrieIds;
        }

        public Guid Id { get; set; }

        public List<string> EntriesIds { get; set; }
    }
}