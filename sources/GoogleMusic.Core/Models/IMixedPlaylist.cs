// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public interface IMixedPlaylist : IPlaylist
    {
        Uri[] ArtUrls { get; set; }
    }
}
