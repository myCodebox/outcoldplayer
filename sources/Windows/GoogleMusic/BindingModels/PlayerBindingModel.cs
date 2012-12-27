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

        private bool isShuffleEnabled = false;
        private bool isRepeatAllEnabled = false;
        private bool isLockScreenEnabled = false;

        public PlayerBindingModel()
        {
            this.Songs = new ObservableCollection<SongBindingModel>();
            this.Songs.CollectionChanged += (sender, args) =>
                {
                    for (int index = 0; index < this.Songs.Count; index++)
                    {
                        var songBindingModel = this.Songs[index];
                        songBindingModel.Index = index + 1;
                    }
                };
        }

        public DelegateCommand SkipBackCommand { get; set; }

        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand PauseCommand { get; set; }

        public DelegateCommand SkipAheadCommand { get; set; }

        public DelegateCommand ShuffleCommand { get; set; }

        public DelegateCommand RepeatAllCommand { get; set; }

        public DelegateCommand LockScreenCommand { get; set; }

        public ObservableCollection<SongBindingModel> Songs { get; private set; }

        public bool IsShuffleEnabled
        {
            get
            {
                return this.isShuffleEnabled;
            }

            set
            {
                if (this.isShuffleEnabled != value)
                {
                    this.isShuffleEnabled = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        public bool IsRepeatAllEnabled
        {
            get
            {
                return this.isRepeatAllEnabled;
            }

            set
            {
                if (this.isRepeatAllEnabled != value)
                {
                    this.isRepeatAllEnabled = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        public bool IsLockScreenEnabled
        {
            get
            {
                return this.isLockScreenEnabled;
            }

            set
            {
                if (this.isLockScreenEnabled != value)
                {
                    this.isLockScreenEnabled = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

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
                    this.UpdateCurrentSong();
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
                    this.UpdateCurrentSong();
                    if (value < this.Songs.Count)
                    {
                        this.currentSongIndex = value;
                        this.RaiseCurrentPropertyChanged();
                        this.RaisePropertyChanged("CurrentSong");
                        this.UpdateCurrentSong();
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

        public void UpdateBindingModel()
        {
            this.PauseCommand.RaiseCanExecuteChanged();
            this.PlayCommand.RaiseCanExecuteChanged();
            this.SkipAheadCommand.RaiseCanExecuteChanged();
            this.SkipBackCommand.RaiseCanExecuteChanged();

            this.UpdateCurrentSong();
        }

        private void UpdateCurrentSong()
        {
            if (this.CurrentSong != null)
            {
                foreach (var songBindingModel in this.Songs)
                {
                    if (songBindingModel != this.CurrentSong)
                    {
                        songBindingModel.IsPlaying = false;
                    }
                }

                this.CurrentSong.IsPlaying = this.State == PlayState.Play;
            }
        }
    }
}