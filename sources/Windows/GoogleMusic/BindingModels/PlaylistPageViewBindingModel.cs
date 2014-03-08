// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistPageViewBindingModel<TPlaylist> : BindingModelBase where TPlaylist : class, IPlaylist
    {
        private TPlaylist playlist;

        private string playlistType;

        private IList<Song> songs;

        public TPlaylist Playlist
        {
            get
            {
                return this.playlist;
            }

            set
            {
                this.SetValue(ref this.playlist, value);
                this.RaisePropertyChanged(() => this.Type);
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

        public string Type
        {
            get
            {
                return this.playlistType;
            }

            set
            {
                this.SetValue(ref this.playlistType, value);
            }
        }
    }
}