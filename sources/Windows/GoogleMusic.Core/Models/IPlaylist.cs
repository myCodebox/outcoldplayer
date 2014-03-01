// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public interface IPlaylist
    {
        string Id { get; }

        PlaylistType PlaylistType { get; }

        string Title { get; set; }

        string TitleNorm { get; set; }

        int SongsCount { get; set; }

        int OfflineSongsCount { get; set; }

        TimeSpan Duration { get; set; }

        TimeSpan OfflineDuration { get; set; }

        Uri ArtUrl { get; set; }

        DateTime Recent { get; set; }
    }
}