// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories.DbModels
{
    using System;

    public interface IPlaylist
    {
        string Title { get; set; }

        string TitleNorm { get; set; }

        int SongsCount { get; set; }

        TimeSpan Duration { get; set; }

        Uri ArtUrl { get; set; }

        DateTime LastPlayed { get; set; }
    }
}