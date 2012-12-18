// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.ObjectModel;

    public class PlayerBindingModel : BindingModelBase
    {
        private int currentSongIndex = -1;

        public PlayerBindingModel()
        {
            this.Songs = new ObservableCollection<SongBindingModel>();
        }

        public ObservableCollection<SongBindingModel> Songs { get; private set; }

        public int CurrentSongIndex
        {
            get
            {
                return this.currentSongIndex;
            }

            set
            {
                if (this.currentSongIndex != value)
                {
                    if (value < this.Songs.Count)
                    {
                        this.currentSongIndex = value;
                        this.RaiseCurrentPropertyChanged();
                        this.RaisePropertyChanged("CurrentSong");
                    }
                }
            }
        }

        public SongBindingModel CurrentSong
        {
            get
            {
                if (this.CurrentSongIndex >= 0 && this.CurrentSongIndex < this.Songs.Count)
                {
                    return this.Songs[this.CurrentSongIndex];
                }

                return null;
            }
        }
    }
}