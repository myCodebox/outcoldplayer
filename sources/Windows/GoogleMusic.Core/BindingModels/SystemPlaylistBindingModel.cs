// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;
    using System.Linq;

    public enum SystemPlaylistType
    {
        AllSongs = 1,

        HighlyRated = 2,

        LastAdded = 3
    }

    public class SystemPlaylistBindingModel : PlaylistBaseBindingModel
    {
        public SystemPlaylistBindingModel(string name, SystemPlaylistType type, IEnumerable<SongBindingModel> songs)
            : base(
            name,
            songs.ToList())
        {
            this.Type = type;
            this.AlbumArtUrl = null;
        }
        
        public SystemPlaylistType Type { get; set; }
    }
}