// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlayerViewPresenter : ViewPresenterBase<IPlayerView>
    {
        private readonly IPlayQueueService queueService;
        private readonly INavigationService navigationService;
        private readonly ISongsService songsService;

        private readonly IAnalyticsService analyticsService;

        private readonly IMediaElementContainer mediaElement;

        private double progressPosition;

        private int visiblePanelIndex;

        public PlayerViewPresenter(
            IMediaElementContainer mediaElementContainer, 
            IGoogleMusicSessionService sessionService, 
            IPlayQueueService queueService, 
            INavigationService navigationService,
            IApplicationSettingViewsService applicationSettingViewsService,
            ISongsService songsService,
            IAnalyticsService analyticsService,
            PlayerBindingModel playerBindingModel,
            ApplicationSize applicationSize)
        {
            this.mediaElement = mediaElementContainer;
            this.queueService = queueService;
            this.navigationService = navigationService;
            this.songsService = songsService;
            this.analyticsService = analyticsService;

            this.BindingModel = playerBindingModel;

            this.SkipBackCommand = new DelegateCommand(this.PreviousSong, () => this.queueService.CanSwitchToPrevious());
            this.PlayCommand = new DelegateCommand(async () => await this.PlayAsync(), () => !this.BindingModel.IsBusy && (this.BindingModel.State == QueueState.Stopped || this.BindingModel.State == QueueState.Paused));
            this.PauseCommand = new DelegateCommand(async () => await this.PauseAsync(), () => !this.BindingModel.IsBusy && this.BindingModel.IsPlaying);
            this.SkipAheadCommand = new DelegateCommand(this.NextSong, () => this.queueService.CanSwitchToNext());
            this.RepeatCommand = new DelegateCommand(() =>
            {
                switch (this.RepeatValue)
                {
                    case RepeatType.None:
                        this.RepeatValue = RepeatType.All;
                        break;
                    case RepeatType.One:
                        this.RepeatValue = RepeatType.None;
                        break;
                    case RepeatType.All:
                        this.RepeatValue = RepeatType.One;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }, () => !this.BindingModel.IsBusy && !queueService.IsRadio);
            this.ShuffleCommand = new DelegateCommand(() => { }, () => !this.BindingModel.IsBusy && !queueService.IsRadio);

            this.BindingModel.Subscribe(
                () => this.BindingModel.CurrentPosition,
                async (sender, args) =>
                {
                    if (this.BindingModel.IsPlaying)
                    {
                        double currentPosition = this.BindingModel.CurrentPosition;
                        if (Math.Abs(this.progressPosition - currentPosition) > 1
                            && (this.BindingModel.DownloadProgress * this.BindingModel.TotalSeconds) >  currentPosition)
                        {
                            await this.mediaElement.SetPositionAsync(TimeSpan.FromSeconds(currentPosition));
                        }
                    }
                });

            this.mediaElement.PlayProgress += (sender, args) =>
            {
                if (this.BindingModel.IsPlaying)
                {
                    try
                    {
                        this.BindingModel.FreezeNotifications();
                        this.BindingModel.TotalSeconds = args.Duration.TotalSeconds;
                        this.BindingModel.CurrentPosition = this.progressPosition = args.Position.TotalSeconds;
                    }
                    finally
                    {
                        this.BindingModel.UnfreezeNotifications();
                    }
                }
            };

            sessionService.SessionCleared += (sender, args) => this.Dispatcher.RunAsync(
                async () =>
                {
                    // TODO: clear playlists
                    await this.StopAsync();
                });

            this.queueService.DownloadProgress += this.CurrentSongStreamOnDownloadProgressChanged;
            this.queueService.QueueChanged += (sender, args) => this.UpdateCommands();
            this.queueService.StateChanged += async (sender, args) => await this.Dispatcher.RunAsync(
                () =>
                {
                    if (args.State == QueueState.Busy || args.State == QueueState.Stopped)
                    {
                        this.BindingModel.CurrentPosition = 0d;
                        this.BindingModel.TotalSeconds = 1.0d;
                    }

                    this.BindingModel.CurrentSong = args.CurrentSong == null ? null : new SongBindingModel(args.CurrentSong);
                    this.BindingModel.State = args.State;
                    this.UpdateCommands();
                });
            
            this.NavigateToQueueView = new DelegateCommand(
                () =>
                {
                    if (this.queueService.CurrentPlaylist != null && !(this.queueService.CurrentPlaylist is Artist))
                    {
                        this.navigationService.NavigateToPlaylist(this.queueService.CurrentPlaylist);
                    }
                    else
                    {
                        this.navigationService.NavigateTo<ICurrentPlaylistPageView>(keepInHistory: false);
                    }
                });

            this.ShowApplicationSettingsCommand = new DelegateCommand(async () =>
            {
                await this.Dispatcher.RunAsync(applicationSettingViewsService.Show);
            });

            this.RateSongCommand = new DelegateCommand(
                parameter =>
                {
                    var ratingEventArgs = parameter as RatingEventArgs;
                    if (this.BindingModel.CurrentSong != null && ratingEventArgs != null)
                    {
                        this.Logger.LogTask(this.songsService.UpdateRatingAsync(
                                this.BindingModel.CurrentSong.Metadata, (byte)ratingEventArgs.Value));
                    }

                    this.VisiblePanelIndex = 0;
                });

            this.SwitchPanelCommand = new DelegateCommand(
                () =>
                {
                    if (this.VisiblePanelIndex == 0 && this.BindingModel.CurrentSong == null)
                    {
                        this.VisiblePanelIndex = 2;
                    }
                    else
                    {
                        this.VisiblePanelIndex = this.VisiblePanelIndex > 2 ? 0 : ++this.VisiblePanelIndex;
                    }

                    this.analyticsService.SendEvent("Player", "SwitchPanel", this.VisiblePanelIndex.ToString());
                });

            applicationSize.Subscribe(() => applicationSize.IsSmall,
                (sender, args) =>
                {
                    this.VisiblePanelIndex = 0;
                });
        }

        public PlayerBindingModel BindingModel { get; private set; }

        public DelegateCommand SkipBackCommand { get; set; }

        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand PauseCommand { get; set; }

        public DelegateCommand SkipAheadCommand { get; set; }
        
        public DelegateCommand NavigateToQueueView { get; set; }

        public DelegateCommand ShuffleCommand { get; set; }

        public DelegateCommand RepeatCommand { get; set; }

        public DelegateCommand ShowApplicationSettingsCommand { get; set; }

        public DelegateCommand RateSongCommand { get; set; }

        public DelegateCommand SwitchPanelCommand { get; set; }

        public int VisiblePanelIndex
        {
            get
            {
                return this.visiblePanelIndex;
            }
            set
            {
                this.SetValue(ref this.visiblePanelIndex, value);
            }
        }

        public bool IsShuffleEnabled
        {
            get
            {
                return this.queueService.IsShuffled;
            }

            set
            {
                this.queueService.IsShuffled = value;
            }
        }

        public RepeatType RepeatValue
        {
            get
            {
                return this.queueService.Repeat;
            }

            set
            {
                if (this.queueService.Repeat != value)
                {
                    this.queueService.Repeat = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.EventAggregator.GetEvent<QueueChangeEvent>().Subscribe(
                async (e) => await this.Dispatcher.RunAsync(() =>
                {
                    this.RaisePropertyChanged(() => this.IsShuffleEnabled);
                    this.RaisePropertyChanged(() => this.RepeatValue);
                }));
        }

        private async void NextSong()
        {
            if (await this.queueService.NextSongAsync())
            {
                this.UpdateCommands();
                this.VisiblePanelIndex = 0;
            }
        }

        private async void PreviousSong()
        {
            if (await this.queueService.PreviousSongAsync())
            {
                this.UpdateCommands();
                this.VisiblePanelIndex = 0;
            }
        }

        private async Task PlayAsync()
        {
            await this.queueService.PlayAsync();
            this.UpdateCommands();
        }

        private async Task PauseAsync()
        {
            await this.queueService.PauseAsync();
            this.UpdateCommands();
        }

        private async Task StopAsync()
        {
            await this.queueService.StopAsync();
            this.UpdateCommands();
        }

        private async void CurrentSongStreamOnDownloadProgressChanged(object sender, double d)
        {
            await this.Dispatcher.RunAsync(
                () =>
                {
                    this.Logger.Info("Download progress changed to {0}", d);
                    this.BindingModel.DownloadProgress = d;
                });
        }

        private void UpdateCommands()
        {
            this.PauseCommand.RaiseCanExecuteChanged();
            this.PlayCommand.RaiseCanExecuteChanged();
            this.SkipAheadCommand.RaiseCanExecuteChanged();
            this.SkipBackCommand.RaiseCanExecuteChanged();
            this.ShuffleCommand.RaiseCanExecuteChanged();
            this.RepeatCommand.RaiseCanExecuteChanged();
        }
    }
}