// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

    public class SongsCache
    {
        public SongEntity[] Songs { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}