// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites.Services
{
    using System;
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class SongStubs
    {
        public const string Artist1 = "Artist 1";
        public const string Artist2 = "Artist 2";
        public const string Artist1Album1 = "Artist 1 Album 1";
        public const string Artist1Album2 = "Artist 1 Album 2";
        public const string Artist2Album1 = "Artist 2 Album 1";

        public static readonly Song Song1Artist1Album1 = new Song()
        {
            ProviderSongId = Guid.NewGuid().ToString(),
            ArtistTitle = Artist1,
            ArtistTitleNorm = Artist1,
            AlbumTitle = Artist1Album1,
            AlbumTitleNorm = Artist1Album1,
            Title = "Song 1 Artist 1 Album 1",
            LastPlayed = DateTime.Now.AddDays(-7)
        };

        public static readonly Song Song2Artist1Album1 = new Song()
        {
            ProviderSongId = Guid.NewGuid().ToString(),
            ArtistTitle = Artist1,
            ArtistTitleNorm = Artist1,
            AlbumTitle = Artist1Album1,
            AlbumTitleNorm = Artist1Album1,
            Title = "Song 2 Artist 1 Album 1",
            LastPlayed = DateTime.Now.AddDays(-6)
        };

        public static readonly Song Song1Artist1Album2 = new Song()
        {
            ProviderSongId = Guid.NewGuid().ToString(),
            ArtistTitle = Artist1,
            ArtistTitleNorm = Artist1,
            AlbumTitle = Artist1Album2,
            AlbumTitleNorm = Artist1Album2,
            Title = "Song 1 Artist 1 Album 2",
            LastPlayed = DateTime.Now.AddDays(-5)
        };

        public static readonly Song Song2Artist1Album2 = new Song()
        {
            ProviderSongId = Guid.NewGuid().ToString(),
            ArtistTitle = Artist1,
            ArtistTitleNorm = Artist1,
            AlbumTitle = Artist1Album2,
            AlbumTitleNorm = Artist1Album2,
            Title = "Song 2 Artist 1 Album 2",
            LastPlayed = DateTime.Now.AddDays(-4)
        };

        public static readonly Song Song1Artist2Album1 = new Song()
        {
            ProviderSongId = Guid.NewGuid().ToString(),
            ArtistTitle = Artist2,
            ArtistTitleNorm = Artist2,
            AlbumTitle = Artist2Album1,
            AlbumTitleNorm = Artist2Album1,
            Title = "Song 1 Artist 2 Album 1",
            LastPlayed = DateTime.Now.AddDays(-3)
        };

        public static readonly Song Song2Artist2Album1 = new Song()
        {
            ProviderSongId = Guid.NewGuid().ToString(),
            ArtistTitle = Artist2,
            ArtistTitleNorm = Artist2,
            AlbumTitle = Artist2Album1,
            AlbumTitleNorm = Artist2Album1,
            Title = "Song 2 Artist 2 Album 1",
            LastPlayed = DateTime.Now.AddDays(-2)
        };

        public static IEnumerable<Song> GetAllSongs()
        {
            return new[]
                       {
                           Song1Artist1Album1, Song2Artist1Album1, Song1Artist1Album2, Song2Artist1Album2,
                           Song1Artist2Album1, Song2Artist2Album1
                       };
        }
    }
}