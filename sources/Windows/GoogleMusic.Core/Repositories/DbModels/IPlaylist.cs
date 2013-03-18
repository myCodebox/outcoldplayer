// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories.DbModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IPlaylist
    {
        int Id { get; }

        PlaylistType PlaylistType { get; }

        string Title { get; set; }

        string TitleNorm { get; set; }

        int SongsCount { get; set; }

        TimeSpan Duration { get; set; }

        Uri ArtUrl { get; set; }

        DateTime LastPlayed { get; set; }
    }
}