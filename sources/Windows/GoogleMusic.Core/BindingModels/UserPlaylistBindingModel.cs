// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public class UserPlaylistBindingModel : PlaylistBaseBindingModel
    {
        public UserPlaylistBindingModel(UserPlaylist metadata, string name, List<SongBindingModel> songs, List<string> entrieIds)
            : base(name, songs)
        {
            Debug.Assert(songs.Count == entrieIds.Count, "songs.Count == entrieIds.Count");

            this.Metadata = metadata;
            this.EntriesIds = entrieIds;
        }

        public UserPlaylist Metadata { get; set; }

        public List<string> EntriesIds { get; set; }
    }
}