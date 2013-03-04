// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.GoogleMusic.Models;

    public enum PlayState
    {
        Stop = 0,

        Play = 1,

        Pause = 2
    }

    public class PlayerBindingModel : SongsBindingModelBase
    {
        private int currentSongIndex = -1;
        private PlayState playState = PlayState.Stop;

        private bool isShuffleEnabled = false;
        private bool isRepeatAllEnabled = false;
        private bool isLockScreenEnabled = false;

        private bool isBusy = false;

        private double totalSeconds = 1;
        private double currentPosition;
        private double downloadProgress;

        public DelegateCommand SkipBackCommand { get; set; }

        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand PauseCommand { get; set; }

        public DelegateCommand SkipAheadCommand { get; set; }

        public DelegateCommand ShuffleCommand { get; set; }

        public DelegateCommand RepeatAllCommand { get; set; }

        public DelegateCommand LockScreenCommand { get; set; }

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
                    this.RaisePropertyChanged(() => this.IsPlaying);
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
                var currentSong = this.CurrentSong;
                if (currentSong != null)
                {
                    currentSong.State = SongState.None;
                }

                this.currentSongIndex = value;

                currentSong = this.CurrentSong;
                if (currentSong != null)
                {
                    currentSong.State = SongState.Playing;
                }

                this.RaiseCurrentPropertyChanged();
                this.RaisePropertyChanged(() => this.CurrentSong);
            }
        }

        public Song CurrentSong
        {
            get
            {
                if (this.currentSongIndex >= 0 && this.currentSongIndex < this.Songs.Count)
                {
                    return this.Songs[this.currentSongIndex];
                }

                return null;
            }
        }

        public bool IsBusy
        {
            get
            {
                return this.isBusy;
            }

            set
            {
                if (this.isBusy != value)
                {
                    this.isBusy = value;
                    this.RaiseCurrentPropertyChanged();
                    this.UpdateBindingModel();
                }
            }
        }

        public double TotalSeconds
        {
            get
            {
                return this.totalSeconds;
            }

            set
            {
                this.SetValue(ref this.totalSeconds, value);
            }
        }

        public double CurrentPosition
        {
            get
            {
                return this.currentPosition;
            }

            set
            {
                this.currentPosition = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public double DownloadProgress
        {
            get
            {
                return this.downloadProgress;
            }

            set
            {
                this.downloadProgress = value;
                this.RaiseCurrentPropertyChanged();
                this.RaisePropertyChanged(() => this.IsDownloaded);
            }
        }

        public bool IsDownloaded
        {
            get
            {
                return this.DownloadProgress <= 0.001 || this.DownloadProgress >= 0.999;
            }
        }

        public void UpdateBindingModel()
        {
            this.PauseCommand.RaiseCanExecuteChanged();
            this.PlayCommand.RaiseCanExecuteChanged();
            this.SkipAheadCommand.RaiseCanExecuteChanged();
            this.SkipBackCommand.RaiseCanExecuteChanged();
        }
    }
}