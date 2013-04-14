// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public static class SystemPlaylistTypeExtensions
    {
        public static string ToTitle(this SystemPlaylistType systemPlaylistType)
        {
            switch (systemPlaylistType)
            {
                case SystemPlaylistType.AllSongs:
                    return "All songs";
                case SystemPlaylistType.HighlyRated:
                    return "Highly rated";
                case SystemPlaylistType.LastAdded:
                    return "Last added";
                default:
                    throw new ArgumentOutOfRangeException("systemPlaylistType");
            }
        }
    }
}
