// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistPageViewBindingModel : BindingModelBase
    {
        private IPlaylist playlist;

        private string title;
        private string subtitle;

        private IList<Song> songs;

        public IPlaylist Playlist
        {
            get
            {
                return this.playlist;
            }

            set
            {
                this.SetValue(ref this.playlist, value);
            }
        }

        public IList<Song> Songs
        {
            get
            {
                return this.songs;
            }

            set
            {
                this.SetValue(ref this.songs, value);
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.SetValue(ref this.title, value);
            }
        }


        public string Subtitle
        {
            get
            {
                return this.subtitle;
            }

            set
            {
                this.SetValue(ref this.subtitle, value);
            }
        }
    }
}