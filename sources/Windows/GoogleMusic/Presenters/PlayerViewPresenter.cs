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
    using OutcoldSolutions.Presenters;

    using Windows.System.Display;

    public class PlayerViewPresenter : ViewPresenterBase<IPlayerView>
    {
        private readonly IGoogleMusicSessionService sessionService;
        private readonly ISettingsService settingsService;
        private readonly IPlayQueueService queueService;
        private readonly IMediaElementContainer mediaElement;
        
        private DisplayRequest request;
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
            this.BindingModel = new PlayerBindingModel
                                    {
                                        IsRepeatAllEnabled = this.queueService.IsRepeatAll,
                                        IsShuffleEnabled = this.queueService.IsShuffled,
                                        IsLockScreenEnabled = this.settingsService.GetValue("IsLockScreenEnabled", defaultValue: false),
                                        Volume = this.mediaElement.Volume
                                    };

            if (this.BindingModel.IsLockScreenEnabled)
            {
                this.UpdateLockScreen();
            }
            
            this.BindingModel.SkipBackCommand = new DelegateCommand(this.PreviousSong, () => !this.BindingModel.IsBusy && this.queueService.CanSwitchToPrevious());
            this.BindingModel.PlayCommand = new DelegateCommand(async () => await this.PlayAsync(), () => !this.BindingModel.IsBusy && (this.BindingModel.State == QueueState.Stopped || this.BindingModel.State == QueueState.Paused));
            this.BindingModel.PauseCommand = new DelegateCommand(async () => await this.PauseAsync(), () => !this.BindingModel.IsBusy && this.BindingModel.IsPlaying);
            this.BindingModel.SkipAheadCommand = new DelegateCommand(this.NextSong, () => !this.BindingModel.IsBusy && this.queueService.CanSwitchToNext());

            this.BindingModel.LockScreenCommand = new DelegateCommand(this.UpdateLockScreen);

            this.BindingModel.RepeatAllCommand = new DelegateCommand(async () => await this.queueService.SetRepeatAllAsync(this.BindingModel.IsRepeatAllEnabled = !this.BindingModel.IsRepeatAllEnabled));
            this.BindingModel.ShuffleCommand = new DelegateCommand(async () => await this.queueService.SetShuffledAsync(this.BindingModel.IsShuffleEnabled));

            this.BindingModel.UpdateBindingModel();

            this.BindingModel.Subscribe(
                () => this.BindingModel.IsShuffleEnabled, 
                (sender, args) => this.queueService.SetShuffledAsync(this.BindingModel.IsShuffleEnabled));

            this.BindingModel.Subscribe(
                () => this.BindingModel.IsRepeatAllEnabled,
                (sender, args) => this.queueService.SetShuffledAsync(this.BindingModel.IsRepeatAllEnabled));

            this.BindingModel.Subscribe(
                () => this.BindingModel.IsLockScreenEnabled,
                (sender, args) => this.settingsService.SetValue("IsLockScreenEnabled", this.BindingModel.IsLockScreenEnabled));

            this.BindingModel.Subscribe(
                () => this.BindingModel.CurrentPosition,
                async (sender, args) =>
                    {
                        if (Math.Abs(this.progressPosition - this.BindingModel.CurrentPosition) > 1)
                        {
                            await this.mediaElement.SetPositionAsync(TimeSpan.FromSeconds(this.BindingModel.CurrentPosition));
                        }
                    });

            this.BindingModel.Subscribe(() => this.BindingModel.Volume, (sender, args) => this.mediaElement.Volume = this.BindingModel.Volume);

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
                        this.BindingModel.CurrentSong = args.CurrentSong;
                        this.BindingModel.State = args.State;
                        this.BindingModel.UpdateBindingModel();
                    });

            this.queueService.QueueChanged += (sender, args) =>
                {
                    this.BindingModel.IsRepeatAllEnabled = this.queueService.IsRepeatAll;
                    this.BindingModel.IsShuffleEnabled = this.queueService.IsShuffled;
                };
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
        
        private void UpdateLockScreen()
        {
            if (this.request == null)
            {
                this.request = new DisplayRequest();
                this.request.RequestActive();
                this.Logger.Debug("Request display active.");
            }
            else
            {
                this.request.RequestRelease();
                this.request = null;
                this.Logger.Debug("Release display active.");
            }

            this.BindingModel.IsLockScreenEnabled = this.request != null;
        }
    }
}