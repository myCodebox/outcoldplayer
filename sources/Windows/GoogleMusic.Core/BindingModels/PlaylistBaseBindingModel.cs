﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.BindingModels;

    public abstract class PlaylistBaseBindingModel : BindingModelBase
    {
        private string title;

        protected PlaylistBaseBindingModel(string name, List<SongBindingModel> songs)
        {
            this.Title = name;
            this.Songs = songs;
            this.CalculateFields();
        }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.title = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public double Duration { get; private set; }

        public Uri AlbumArtUrl { get; protected set; }

        public List<SongBindingModel> Songs { get; private set; }

        public void CalculateFields()
        {
            this.Duration = TimeSpan.FromSeconds(this.Songs.Sum(x => x.Duration)).TotalSeconds;

            var song = this.Songs.FirstOrDefault(x => x.Metadata.AlbumArtUrl != null);
            if (song != null)
            {
                this.AlbumArtUrl = song.Metadata.AlbumArtUrl;
            }
        }
    }
}