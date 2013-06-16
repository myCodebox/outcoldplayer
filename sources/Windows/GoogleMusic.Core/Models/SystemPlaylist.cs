// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public class SystemPlaylist : IPlaylist
    {
        public SystemPlaylistType SystemPlaylistType { get; set; }

        public string Id
        {
            get
            {
                return this.SystemPlaylistType.ToString();
            }
        }

        public PlaylistType PlaylistType 
        {
            get
            {
                return PlaylistType.SystemPlaylist;
            }
        }

        public string Title
        {
            get
            {
                return this.SystemPlaylistType.ToTitle();
            }

            set
            {
            }
        }

        public string TitleNorm { get; set; }

        public int SongsCount { get; set; }

        public int OfflineSongsCount { get; set; }

        public TimeSpan Duration { get; set; }

        public TimeSpan OfflineDuration { get; set; }

        public Uri ArtUrl { get; set; }

        public DateTime LastPlayed { get; set; }
    }
}
