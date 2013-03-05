// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;

    public class PlayerBindingModel : BindingModelBase
    {
        private QueueState playState = QueueState.Unknown;

        private bool isShuffleEnabled = false;
        private bool isRepeatAllEnabled = false;
        private bool isLockScreenEnabled = false;

        private double totalSeconds = 1;
        private double currentPosition;
        private double downloadProgress;
        private double volume;

        private Song currentSong;

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
                this.SetValue(ref this.isShuffleEnabled, value);
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
                this.SetValue(ref this.isRepeatAllEnabled, value);
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
                this.SetValue(ref this.isLockScreenEnabled, value);
            }
        }

        public bool IsPlaying
        {
            get
            {
                return this.playState == QueueState.Play;
            }
        }

        public QueueState State
        {
            get
            {
                return this.playState;
            }

            set
            {
                if (this.SetValue(ref this.playState, value))
                {
                    this.RaisePropertyChanged(() => this.IsPlaying);
                    this.RaisePropertyChanged(() => this.IsBusy);
                }
            }
        }

        public Song CurrentSong
        {
            get
            {
                return this.currentSong;
            }

            set
            {
                this.SetValue(ref this.currentSong, value);
            }
        }

        public bool IsBusy
        {
            get
            {
                return this.playState == QueueState.Busy;
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
                this.SetValue(ref this.currentPosition, value);
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
                if (this.SetValue(ref this.downloadProgress, value))
                {
                    this.RaisePropertyChanged(() => this.IsDownloaded);
                }
            }
        }

        public bool IsDownloaded
        {
            get
            {
                return this.DownloadProgress <= 0.001 || this.DownloadProgress >= 0.999;
            }
        }

        public double Volume
        {
            get
            {
                return this.volume;
            }

            set
            {
                this.SetValue(ref this.volume, value);
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