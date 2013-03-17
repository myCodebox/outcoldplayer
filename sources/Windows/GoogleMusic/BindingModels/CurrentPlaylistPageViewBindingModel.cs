// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class CurrentPlaylistPageViewBindingModel : BindingModelBase
    {
        private int selectedSongIndex;
        private List<SongBindingModel> songs;

        public List<SongBindingModel> Songs
        {
            get
            {
                return this.songs;
            }

            set
            {
                this.songs = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public SongBindingModel SelectedSong
        {
            get
            {
                if (0 <= this.SelectedSongIndex && this.SelectedSongIndex < this.Songs.Count)
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