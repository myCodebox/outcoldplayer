// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public class SystemPlaylist : IPlaylist
    {
        public SystemPlaylistType SystemPlaylistType { get; set; }

        public string Title { get; set; }

        public string TitleNorm { get; set; }

        public int SongsCount { get; set; }

        public TimeSpan Duration { get; set; }

        public Uri ArtUrl { get; set; }

        public DateTime LastPlayed { get; set; }
    }
}
