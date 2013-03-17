// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    
    public class SystemPlaylist
    {
        public SystemPlaylistType SystemPlaylistType { get; set; }

        public int SongsCount { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
