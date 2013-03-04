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

    using Windows.Media;
    using Windows.System.Display;

    public class PlayerViewPresenter : ViewPresenterBase<IPlayerView>, IDisposable
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
                                        IsLockScreenEnabled = this.settingsService.GetValue("IsLockScreenEnabled", defaultValue: false)
                                    };

            if (this.BindingModel.IsLockScreenEnabled)
            {
                this.UpdateLockScreen();
            }

            MediaControl.PlayPauseTogglePressed += this.MediaControlPlayPauseTogglePressed;
            MediaControl.PlayPressed += this.MediaControlPlayPressed;
            MediaControl.PausePressed += this.MediaControlPausePressed;
            MediaControl.StopPressed += this.MediaControlStopPressed;

            this.BindingModel.SkipBackCommand = new DelegateCommand(this.PreviousSong, () => !this.BindingModel.IsBusy && this.queueService.CanSwitchToPrevious());
            this.BindingModel.PlayCommand = new DelegateCommand(async () => await this.PlayAsync(), () => !this.BindingModel.IsBusy && !this.BindingModel.IsPlaying && this.BindingModel.Songs.Count > 0);
            this.BindingModel.PauseCommand = new DelegateCommand(async () => await this.PauseAsync(), () => !this.BindingModel.IsBusy && this.BindingModel.IsPlaying);
            this.BindingModel.SkipAheadCommand = new DelegateCommand(this.NextSong, () => !this.BindingModel.IsBusy && this.queueService.CanSwitchToNext());

            this.BindingModel.LockScreenCommand = new DelegateCommand(this.UpdateLockScreen);

            this.BindingModel.RepeatAllCommand = new DelegateCommand(async () => await this.queueService.SetRepeatAllAsync(this.BindingModel.IsRepeatAllEnabled = !this.BindingModel.IsRepeatAllEnabled));
            this.BindingModel.ShuffleCommand = new DelegateCommand(async () => await this.queueService.SetShuffledAsync(this.BindingModel.IsShuffleEnabled));

            this.BindingModel.UpdateBindingModel();

            this.BindingModel.PropertyChanged += async (sender, args) =>
                {
                    if (args.PropertyName.Equals("CurrentPosition"))
                    {
                        if (Math.Abs(progressPosition - this.BindingModel.CurrentPosition) > 1)
                        {
                            await this.mediaElement.SetPositionAsync(TimeSpan.FromSeconds(this.BindingModel.CurrentPosition));
                        }
                    }
                    else if (args.PropertyName.Equals("IsRepeatAllEnabled"))
                    {
                        this.settingsService.SetValue("IsRepeatAllEnabled", this.BindingModel.IsRepeatAllEnabled);
                    }
                    else if (args.PropertyName.Equals("IsShuffleEnabled"))
                    {
                        this.settingsService.SetValue("IsShuffleEnabled", this.BindingModel.IsShuffleEnabled);
                    }
                    else if (args.PropertyName.Equals("IsLockScreenEnabled"))
                    {
                        this.settingsService.SetValue("IsLockScreenEnabled", this.BindingModel.IsLockScreenEnabled);
                    }
                };

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
                        this.UpdateMediaButtons();
                    });
        }

        ~PlayerViewPresenter()
        {
            this.Dispose(disposing: false);
        }

        public PlayerBindingModel BindingModel { get; private set; }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

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

        private void UpdateMediaButtons()
        {
            if (this.BindingModel.SkipAheadCommand.CanExecute())
            {
                MediaControl.NextTrackPressed -= this.MediaControlOnNextTrackPressed;
                MediaControl.NextTrackPressed += this.MediaControlOnNextTrackPressed;
            }
            else
            {
                MediaControl.NextTrackPressed -= this.MediaControlOnNextTrackPressed;
            }

            if (this.BindingModel.SkipBackCommand.CanExecute())
            {
                MediaControl.PreviousTrackPressed -= this.MediaControlOnPreviousTrackPressed;
                MediaControl.PreviousTrackPressed += this.MediaControlOnPreviousTrackPressed;
            }
            else
            {
                MediaControl.PreviousTrackPressed -= this.MediaControlOnPreviousTrackPressed;
            }
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

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                MediaControl.PlayPauseTogglePressed -= this.MediaControlPlayPauseTogglePressed;
                MediaControl.PlayPressed -= this.MediaControlPlayPressed;
                MediaControl.PausePressed -= this.MediaControlPausePressed;
                MediaControl.StopPressed -= this.MediaControlStopPressed;
            }
        }

        private async void MediaControlPausePressed(object sender, object e)
        {
            this.Logger.Debug("MediaControlPausePressed.");
            await this.PauseAsync();
        }

        private async void MediaControlPlayPressed(object sender, object e)
        {
            this.Logger.Debug("MediaControlPlayPressed.");
            await this.PlayAsync();
        }

        private async void MediaControlStopPressed(object sender, object e)
        {
            this.Logger.Debug("MediaControlStopPressed.");
            await this.StopAsync();
        }

        private async void MediaControlPlayPauseTogglePressed(object sender, object e)
        {
            this.Logger.Debug("MediaControlPlayPauseTogglePressed.");
            if (this.BindingModel.State == QueueState.Play)
            {
                await this.PauseAsync();
            }
            else
            {
                await this.PlayAsync();
            }
        }

        private void MediaControlOnNextTrackPressed(object sender, object o)
        {
            this.Logger.Debug("MediaControlOnNextTrackPressed.");
            this.NextSong();
        }

        private void MediaControlOnPreviousTrackPressed(object sender, object o)
        {
            this.Logger.Debug("MediaControlOnPreviousTrackPressed.");
            this.Dispatcher.RunAsync(this.PreviousSong);
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