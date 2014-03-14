// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;
    using System.Reactive.Linq;

    using OutcoldSolutions.GoogleMusic.EventAggregator;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;

    public class PlayerBindingModel : BindingModelBase
    {
        private readonly IMediaElementContainer mediaElementContainer;

        private QueueState playState = QueueState.Unknown;

        private double totalSeconds = 1;
        private double currentPosition;
        private double downloadProgress;

        private SongBindingModel currentSong;

        public PlayerBindingModel(
            IMediaElementContainer mediaElementContainer,
            IDispatcher dispatcher,
            IEventAggregator eventAggregator)
        {
            this.mediaElementContainer = mediaElementContainer;
            eventAggregator.GetEvent<SongsUpdatedEvent>()
                .Where(e => e.UpdatedSongs != null)
                .Subscribe(
                    async e =>
                    {
                        SongBindingModel songBindingModel = this.CurrentSong;
                        if (songBindingModel != null)
                        {
                            foreach (var song in e.UpdatedSongs)
                            {
                                if (string.Equals(
                                    song.SongId,
                                    songBindingModel.Metadata.SongId,
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    var bindingModel = new SongBindingModel(song);
                                    await dispatcher.RunAsync(
                                        () =>
                                        {
                                            this.CurrentSong = bindingModel;
                                        });
                                    return;
                                }
                            }
                        }
                    });
        }

        public bool IsPlaying
        {
            get
            {
                return this.playState == QueueState.Play;
            }
        }

        public double Volume
        {
            get
            {
                return this.mediaElementContainer.Volume;
            }

            set
            {
                this.mediaElementContainer.Volume = value;
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

        public SongBindingModel CurrentSong
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
    }
}