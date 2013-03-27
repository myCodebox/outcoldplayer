// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    public class PlayerViewPresenter : ViewPresenterBase<IPlayerView>
    {
        private readonly IGoogleMusicSessionService sessionService;
        private readonly IPlayQueueService queueService;
        private readonly INavigationService navigationService;
        private readonly IMediaElementContainer mediaElement;
        
        private double progressPosition;

        public PlayerViewPresenter(
            IMediaElementContainer mediaElementContainer,
            IGoogleMusicSessionService sessionService,
            IPlayQueueService queueService,
            INavigationService navigationService)
        {
            this.mediaElement = mediaElementContainer;
            this.sessionService = sessionService;
            this.queueService = queueService;
            this.navigationService = navigationService;

            this.BindingModel = new PlayerBindingModel();
            
            this.SkipBackCommand = new DelegateCommand(this.PreviousSong, () => !this.BindingModel.IsBusy && this.queueService.CanSwitchToPrevious());
            this.PlayCommand = new DelegateCommand(async () => await this.PlayAsync(), () => !this.BindingModel.IsBusy && (this.BindingModel.State == QueueState.Stopped || this.BindingModel.State == QueueState.Paused));
            this.PauseCommand = new DelegateCommand(async () => await this.PauseAsync(), () => !this.BindingModel.IsBusy && this.BindingModel.IsPlaying);
            this.SkipAheadCommand = new DelegateCommand(this.NextSong, () => !this.BindingModel.IsBusy && this.queueService.CanSwitchToNext());

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
                        this.BindingModel.TotalSeconds = args.Duration.TotalSeconds;
                        this.BindingModel.CurrentPosition = this.progressPosition = args.Position.TotalSeconds;
                    }
                };
           
            this.sessionService.SessionCleared += (sender, args) => this.Dispatcher.RunAsync(
                async () =>
                    {
                        // TODO: clear playlists
                        await this.StopAsync();
                    });

            this.queueService.DownloadProgress += this.CurrentSongStreamOnDownloadProgressChanged;

            this.queueService.StateChanged += async (sender, args) => await this.Dispatcher.RunAsync(
                () =>
                    {
                        if (args.State == QueueState.Busy || args.State == QueueState.Stopped)
                        {
                            this.BindingModel.CurrentPosition = 0d;
                            this.BindingModel.TotalSeconds = 0d;
                        }

                        this.BindingModel.CurrentSong = args.CurrentSong;
                        this.BindingModel.State = args.State;
                        this.UpdateCommands();
                    });

            this.ShowMoreCommand = new DelegateCommand(() => this.MainFrame.ShowPopup<IPlayerMorePopupView>(PopupRegion.AppToolBarRight));
            this.NavigateToQueueView = new DelegateCommand(() => this.navigationService.NavigateTo<ICurrentPlaylistPageView>().SelectPlayingSong());
        }

        public PlayerBindingModel BindingModel { get; private set; }

        public DelegateCommand SkipBackCommand { get; set; }

        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand PauseCommand { get; set; }

        public DelegateCommand SkipAheadCommand { get; set; }

        public DelegateCommand ShowMoreCommand { get; set; }

        public DelegateCommand NavigateToQueueView { get; set; }

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
        }
    }
}