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

    using Windows.System.Display;

    public class PlayerViewPresenter : ViewPresenterBase<IPlayerView>
    {
        private readonly IGoogleMusicSessionService sessionService;
        private readonly ISettingsService settingsService;
        private readonly IPlayQueueService queueService;
        private readonly IMediaElementContainer mediaElement;
        
        private double progressPosition;

        public PlayerViewPresenter(
            IMediaElementContainer mediaElementContainer,
            IGoogleMusicSessionService sessionService,
            ISettingsService settingsService,
            IPlayQueueService queueService)
        {
            this.mediaElement = mediaElementContainer;
            this.sessionService = sessionService;
            this.settingsService = settingsService;
            this.queueService = queueService;
            
            this.BindingModel.SkipBackCommand = new DelegateCommand(this.PreviousSong, () => !this.BindingModel.IsBusy && this.queueService.CanSwitchToPrevious());
            this.BindingModel.PlayCommand = new DelegateCommand(async () => await this.PlayAsync(), () => !this.BindingModel.IsBusy && (this.BindingModel.State == QueueState.Stopped || this.BindingModel.State == QueueState.Paused));
            this.BindingModel.PauseCommand = new DelegateCommand(async () => await this.PauseAsync(), () => !this.BindingModel.IsBusy && this.BindingModel.IsPlaying);
            this.BindingModel.SkipAheadCommand = new DelegateCommand(this.NextSong, () => !this.BindingModel.IsBusy && this.queueService.CanSwitchToNext());

            this.BindingModel.UpdateBindingModel();

            this.BindingModel.Subscribe(
                () => this.BindingModel.CurrentPosition,
                async (sender, args) =>
                    {
                        if (Math.Abs(this.progressPosition - this.BindingModel.CurrentPosition) > 1)
                        {
                            await this.mediaElement.SetPositionAsync(TimeSpan.FromSeconds(this.BindingModel.CurrentPosition));
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
                        this.BindingModel.CurrentSong = args.CurrentSong == null ? null : new SongBindingModel(args.CurrentSong);
                        this.BindingModel.State = args.State;
                        this.BindingModel.UpdateBindingModel();
                    });

            this.BindingModel.ShowMoreCommand = new DelegateCommand(() =>
                {
                    this.MainFrame.ShowPopup<IPlayerMorePopupView>(PopupRegion.AppToolBarRight);
                });
        }

        public PlayerBindingModel BindingModel { get; private set; }
        
        private async void NextSong()
        {
            await this.queueService.NextSongAsync();
            this.BindingModel.UpdateBindingModel();
        }

        private async void PreviousSong()
        {
            await this.queueService.PreviousSongAsync();
            this.BindingModel.UpdateBindingModel();
        }

        private async Task PlayAsync()
        {
            await this.queueService.PlayAsync();
            this.BindingModel.UpdateBindingModel();
        }

        private async Task PauseAsync()
        {
            await this.queueService.PauseAsync();
            this.BindingModel.UpdateBindingModel();
        }

        private async Task StopAsync()
        {
            await this.queueService.StopAsync();
            this.BindingModel.UpdateBindingModel();
        }
        
        private void CurrentSongStreamOnDownloadProgressChanged(object sender, double d)
        {
            this.Dispatcher.RunAsync(
                () =>
                    {
                        this.Logger.Info("Download progress changed to {0}", d);
                        this.BindingModel.DownloadProgress = d;
                    });
        }
    }
}