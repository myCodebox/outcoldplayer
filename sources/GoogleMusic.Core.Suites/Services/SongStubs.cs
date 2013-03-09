// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites.Services
{
    using System;
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class SongStubs
    {
        public const string Artist1 = "Artist 1";
        public const string Artist2 = "Artist 2";
        public const string Artist1Album1 = "Artist 1 Album 1";
        public const string Artist1Album2 = "Artist 1 Album 2";
        public const string Artist2Album1 = "Artist 2 Album 1";

        public static readonly Song Song1Artist1Album1 = new Song(new GoogleMusicSong()
        {
            Id = Guid.NewGuid().ToString(),
            Artist = Artist1,
            ArtistNorm = Artist1,
            Album = Artist1Album1,
            AlbumNorm = Artist1Album1,
            Title = "Song 1 Artist 1 Album 1",
            LastPlayed = 1
        });

        public static readonly Song Song2Artist1Album1 = new Song(new GoogleMusicSong()
        {
            Id = Guid.NewGuid().ToString(),
            Artist = Artist1,
            ArtistNorm = Artist1,
            Album = Artist1Album1,
            AlbumNorm = Artist1Album1,
            Title = "Song 2 Artist 1 Album 1",
            LastPlayed = 2
        });

        public static readonly Song Song1Artist1Album2 = new Song(new GoogleMusicSong()
        {
            Id = Guid.NewGuid().ToString(),
            Artist = Artist1,
            ArtistNorm = Artist1,
            Album = Artist1Album2,
            AlbumNorm = Artist1Album2,
            Title = "Song 1 Artist 1 Album 2",
            LastPlayed = 3
        });

        public static readonly Song Song2Artist1Album2 = new Song(new GoogleMusicSong()
        {
            Id = Guid.NewGuid().ToString(),
            Artist = Artist1,
            ArtistNorm = Artist1,
            Album = Artist1Album2,
            AlbumNorm = Artist1Album2,
            Title = "Song 2 Artist 1 Album 2",
            LastPlayed = 4
        });

        public static readonly Song Song1Artist2Album1 = new Song(new GoogleMusicSong()
        {
            Id = Guid.NewGuid().ToString(),
            Artist = Artist2,
            ArtistNorm = Artist2,
            Album = Artist2Album1,
            AlbumNorm = Artist2Album1,
            Title = "Song 1 Artist 2 Album 1",
            LastPlayed = 5
        });

        public static readonly Song Song2Artist2Album1 = new Song(new GoogleMusicSong()
        {
            Id = Guid.NewGuid().ToString(),
            Artist = Artist2,
            ArtistNorm = Artist2,
            Album = Artist2Album1,
            AlbumNorm = Artist2Album1,
            Title = "Song 2 Artist 2 Album 1",
            LastPlayed = 6
        });

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