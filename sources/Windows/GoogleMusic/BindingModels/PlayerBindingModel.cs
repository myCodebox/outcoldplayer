// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.ObjectModel;

    public enum PlayState
    {
        Stop = 0,

        Play = 1,

        Pause = 2
    }

    public class PlayerBindingModel : BindingModelBase
    {
        private int currentSongIndex = -1;
        private PlayState playState = PlayState.Stop;

        public PlayerBindingModel()
        {
            this.Songs = new ObservableCollection<SongBindingModel>();
        }

        public DelegateCommand SkipBackCommand { get; set; }

        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand PauseCommand { get; set; }

        public DelegateCommand SkipAheadCommand { get; set; }

        public ObservableCollection<SongBindingModel> Songs { get; private set; }

        public bool IsPlaying
        {
            get
            {
                return this.playState == PlayState.Play;
            }
        }

        public PlayState State
        {
            get
            {
                return this.playState;
            }

            set
            {
                if (this.playState != value)
                {
                    this.playState = value;
                    this.RaiseCurrentPropertyChanged();
                    this.RaisePropertyChanged("IsPlaying");
                }
            }
        }

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

        public void UpdateCommands()
        {
            this.PauseCommand.RaiseCanExecuteChanged();
            this.PlayCommand.RaiseCanExecuteChanged();
            this.SkipAheadCommand.RaiseCanExecuteChanged();
            this.SkipBackCommand.RaiseCanExecuteChanged();
        }
    }
}