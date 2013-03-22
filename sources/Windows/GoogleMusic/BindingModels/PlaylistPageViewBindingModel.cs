// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistPageViewBindingModel<TPlaylist> : BindingModelBase where TPlaylist : class, IPlaylist
    {
        private TPlaylist playlist;
        private IList<SongBindingModel> songs;
        private int selectedSongIndex = -1;

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

        public IList<SongBindingModel> Songs
        {
            get
            {
                return this.songs;
            }

            set
            {
                this.SetValue(ref this.songs, value);
                this.SelectedSongIndex = -1;
            }
        }

        public string Type
        {
            get
            {
                if (this.Playlist == null)
                {
                    return null;
                }

                // TODO:  Create converter for PlaylistType
                return this.Playlist.PlaylistType.ToTitle();
            }
        }

        public SongBindingModel SelectedSong
        {
            get
            {
                if (this.SelectedSongIndex >= 0 && this.SelectedSongIndex < this.Songs.Count)
                {
                    return this.Songs[this.SelectedSongIndex];
                }

                return null;
            }
        }

        public int SelectedSongIndex
        {
            get
            {
                return this.selectedSongIndex;
            }

            set
            {
                this.selectedSongIndex = value;
                this.RaiseCurrentPropertyChanged();
                this.RaisePropertyChanged(() => this.SelectedSong);
            }
        }
    }
}