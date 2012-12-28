// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    public enum PlayState
    {
        Stop = 0,

        Play = 1,

        Pause = 2
    }

    public class PlayerBindingModel : SongsBindingModelBase
    {
        private string currentSongId = null;
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
                    this.RaisePropertyChanged("IsPlaying");
                }
            }
        }

        public string CurrentSongId
        {
            get
            {
                return this.currentSongId;
            }

            set
            {
                if (!string.Equals(this.currentSongId, value, StringComparison.OrdinalIgnoreCase))
                {
                    this.currentSongId = value;
                    this.RaiseCurrentPropertyChanged();
                    this.RaisePropertyChanged("CurrentSong");
                }
            }
        }

        public SongBindingModel CurrentSong
        {
            get
            {
                return this.Songs.FirstOrDefault(x => string.Equals(this.currentSongId, x.GetSong().Id, StringComparison.OrdinalIgnoreCase));
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
                this.totalSeconds = value;
                this.RaiseCurrentPropertyChanged();
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
                this.RaisePropertyChanged("IsDownloaded");
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