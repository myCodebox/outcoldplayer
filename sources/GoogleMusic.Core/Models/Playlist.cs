// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class Playlist
    {
        public Playlist(string name, List<GoogleMusicSong> songs)
        {
            this.Title = name;
            this.Songs = songs;
            this.Duration = TimeSpan.FromMilliseconds(songs.Sum(x => x.DurationMillis)).TotalSeconds;

            var song = songs.FirstOrDefault(x => !string.IsNullOrEmpty(x.AlbumArtUrl));
            if (song != null)
            {
                this.AlbumArtUrl = song.AlbumArtUrl;
            }
        }

        public string Title { get; protected set; }

        public double Duration { get; private set; }

        public string AlbumArtUrl { get; private set; }

        public List<GoogleMusicSong> Songs { get; private set; }
    }
}