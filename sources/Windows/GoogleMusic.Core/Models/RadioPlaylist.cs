// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public class RadioPlaylist : IPlaylist
    {
        public string Id { get; set; }

        public PlaylistType PlaylistType
        {
            get
            {
                return PlaylistType.Radio;
            }
        }

        public string Title { get; set; }

        public string TitleNorm { get; set; }

        public int SongsCount { get; set; }

        public int OfflineSongsCount { get; set; }

        public TimeSpan Duration { get; set; }

        public TimeSpan OfflineDuration { get; set; }

        public Uri ArtUrl { get; set; }

        public DateTime LastPlayed { get; set; }
    }
}