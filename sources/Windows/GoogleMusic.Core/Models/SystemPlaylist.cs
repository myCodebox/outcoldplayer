// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public class SystemPlaylist : IPlaylist
    {
        public SystemPlaylistType SystemPlaylistType { get; set; }

        public int Id
        {
            get
            {
                return (int)this.SystemPlaylistType;
            }
        }

        public PlaylistType PlaylistType 
        {
            get
            {
                return PlaylistType.SystemPlaylist;
            }
        }

        public string Title { get; set; }

        public string TitleNorm { get; set; }

        public int SongsCount { get; set; }

        public TimeSpan Duration { get; set; }

        public Uri ArtUrl { get; set; }

        public DateTime LastPlayed { get; set; }
    }
}
