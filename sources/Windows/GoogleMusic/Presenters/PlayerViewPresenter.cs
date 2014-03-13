// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class PlayerViewPresenter : ViewPresenterBase<IPlayerView>
    {
        private readonly IPlayQueueService queueService;
        private readonly INavigationService navigationService;
        private readonly IMediaElementContainer mediaElement;

        private double progressPosition;

        public PlayerViewPresenter(
            IMediaElementContainer mediaElementContainer, 
            IGoogleMusicSessionService sessionService, 
            IPlayQueueService queueService, 
            INavigationService navigationService, 
            PlayerBindingModel playerBindingModel)
        {
            this.mediaElement = mediaElementContainer;
            this.queueService = queueService;
            this.navigationService = navigationService;

            this.BindingModel = playerBindingModel;

            this.SkipBackCommand = new DelegateCommand(this.PreviousSong, () => this.queueService.CanSwitchToPrevious());
            this.PlayCommand = new DelegateCommand(async () => await this.PlayAsync(), () => !this.BindingModel.IsBusy && (this.BindingModel.State == QueueState.Stopped || this.BindingModel.State == QueueState.Paused));
            this.PauseCommand = new DelegateCommand(async () => await this.PauseAsync(), () => !this.BindingModel.IsBusy && this.BindingModel.IsPlaying);
            this.SkipAheadCommand = new DelegateCommand(this.NextSong, () => this.queueService.CanSwitchToNext());
            this.RepeatAllCommand = new DelegateCommand(() => { }, () => !this.BindingModel.IsBusy && !queueService.IsRadio);
            this.ShuffleCommand = new DelegateCommand(() => { }, () => !this.BindingModel.IsBusy && !queueService.IsRadio);

            this.BindingModel.Subscribe(
                () => this.BindingModel.CurrentPosition,
                async (sender, args) =>
                {
                    if (this.BindingModel.IsPlaying)
                    {
                        double currentPosition = this.BindingModel.CurrentPosition;
                        if (Math.Abs(this.progressPosition - currentPosition) > 1)
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
                    if (this.queueService.CurrentPlaylist != null)
                    {
                        this.navigationService.NavigateToPlaylist(this.queueService.CurrentPlaylist);
                    }
                    else
                    {
                        this.navigationService.NavigateTo<ICurrentPlaylistPageView>(keepInHistory: false);
                    }
                });
            this.ShowMoreCommand = new DelegateCommand(() => this.MainFrame.ShowPopup<IPlayerMorePopupView>(PopupRegion.AppToolBarRight));
        }

        public PlayerBindingModel BindingModel { get; private set; }

        public DelegateCommand SkipBackCommand { get; set; }

        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand PauseCommand { get; set; }

        public DelegateCommand SkipAheadCommand { get; set; }
        
        public DelegateCommand NavigateToQueueView { get; set; }

        public DelegateCommand ShuffleCommand { get; set; }

        public DelegateCommand RepeatAllCommand { get; set; }

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

        public bool IsRepeatAllEnabled
        {
            get
            {
                return this.queueService.IsRepeatAll;
            }

            set
            {
                this.queueService.IsRepeatAll = value;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.EventAggregator.GetEvent<QueueChangeEvent>().Subscribe(
                async (e) => await this.Dispatcher.RunAsync(() =>
                {
                    this.RaisePropertyChanged(() => this.IsShuffleEnabled);
                    this.RaisePropertyChanged(() => this.IsRepeatAllEnabled);
                }));
        }

        private async void NextSong()
        {
            await this.queueService.NextSongAsync();
            this.UpdateCommands();
        }

        private async void PreviousSong()
        {
            await this.queueService.PreviousSongAsync();
            this.UpdateCommands();
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
            this.RepeatAllCommand.RaiseCanExecuteChanged();
        }

        public DelegateCommand ShowMoreCommand { get; set; }
    }
}