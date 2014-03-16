// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public class AllAccessGenre : IMixedPlaylist
    {
        public string Id { get; set; }

        public PlaylistType PlaylistType
        {
            get
            {
                return PlaylistType.AllAccessGenre;
            }
        }

        public string Title { get; set; }

        public string TitleNorm { get; set; }

        public int SongsCount { get; set; }

        public int OfflineSongsCount { get; set; }

        public TimeSpan Duration { get; set; }

        public TimeSpan OfflineDuration { get; set; }

        public Uri ArtUrl { get; set; }

        public DateTime Recent { get; set; }

        public Uri[] ArtUrls { get; set; }

        public string ParentId { get; set; }

        public string[] Children { get; set; }
    }
}
