// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Playlist
    {
        public Playlist(string name, List<Song> songs)
        {
            this.Title = name;
            this.Songs = songs;
            this.Duration = TimeSpan.FromSeconds(songs.Sum(x => x.Duration)).TotalSeconds;

            var song = songs.FirstOrDefault(x => !string.IsNullOrEmpty(x.GoogleMusicMetadata.AlbumArtUrl));
            if (song != null)
            {
                this.AlbumArtUrl = song.GoogleMusicMetadata.AlbumArtUrl;
            }
        }

        public string Title { get; protected set; }

        public double Duration { get; private set; }

        public string AlbumArtUrl { get; private set; }

        public List<Song> Songs { get; private set; }
    }
}