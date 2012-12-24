﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;

    public class SongBindingModel
    {
        private readonly GoogleMusicSong song;

        public SongBindingModel(GoogleMusicSong song)
        {
            this.song = song;
        }

        public string Title
        {
            get
            {
                return this.song.Title;
            }
        }

        public string Artist
        {
            get
            {
                return this.song.Artist;
            }
        }

        public string Album
        {
            get
            {
                return this.song.Album;
            }
        }

        public int Plays
        {
            get
            {
                return this.song.PlayCount;
            }
        }

        public string Time
        {
            get
            {
                var timeSpan = TimeSpan.FromMilliseconds(this.song.DurationMillis);
                return string.Format("{0:N0}:{1:00}", timeSpan.TotalMinutes, timeSpan.Seconds);
            }
        }

        public int Raiting
        {
            get
            {
                return this.song.Rating;
            }
        }

        public ImageSource AlbumArt
        {
            get
            {
                if (this.song.AlbumArtUrl != null)
                {
                    // TODO: Load only 40x40 image
                    return new BitmapImage(new Uri("https:" + this.song.AlbumArtUrl));
                }

                // TODO: Some default image
                return null;
            }
        }

        public GoogleMusicSong GetSong()
        {
            return this.song;
        }
    }
}