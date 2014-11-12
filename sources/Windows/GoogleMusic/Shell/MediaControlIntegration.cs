// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.Media;

    public interface IMediaControlIntegration
    {
        SystemMediaTransportControls GetSystemMediaTransportControls();
    }

    public class MediaControlIntegration : IDisposable, IMediaControlIntegration
    {
        private readonly IPlayQueueService playQueueService;
        private readonly IDispatcher dispatcher;
        private readonly ILogger logger;

        private readonly SystemMediaTransportControls systemMediaTransportControls;

        public MediaControlIntegration(
            ILogManager logManager,
            IPlayQueueService playQueueService,
            IDispatcher dispatcher)
        {
            this.logger = logManager.CreateLogger("MediaControlIntegration");
            this.playQueueService = playQueueService;
            this.dispatcher = dispatcher;
            this.playQueueService.StateChanged += this.StateChanged;

            this.systemMediaTransportControls = SystemMediaTransportControls.GetForCurrentView();
            this.systemMediaTransportControls.ButtonPressed += this.OnButtonPressed;
        }

        ~MediaControlIntegration()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this.systemMediaTransportControls.ButtonPressed -= this.OnButtonPressed;
            GC.SuppressFinalize(this);
        }

        public SystemMediaTransportControls GetSystemMediaTransportControls()
        {
            return this.systemMediaTransportControls;
        }

        private async void StateChanged(object sender, StateChangedEventArgs eventArgs)
        {
            await this.dispatcher.RunAsync(
                () =>
                {
                    switch (eventArgs.State)
                    {
                        case QueueState.Unknown:
                            this.systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Changing;
                            break;
                        case QueueState.Play:
                            this.systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                            break;
                        case QueueState.Stopped:
                            this.systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                            break;
                        case QueueState.Paused:
                            this.systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                            break;
                        case QueueState.Busy:
                            this.systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Changing;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    this.systemMediaTransportControls.IsChannelDownEnabled = false;
                    this.systemMediaTransportControls.IsChannelUpEnabled = false;
                    this.systemMediaTransportControls.IsFastForwardEnabled = false;
                    this.systemMediaTransportControls.IsNextEnabled = this.playQueueService.CanSwitchToNext();
                    this.systemMediaTransportControls.IsPauseEnabled = true;
                    this.systemMediaTransportControls.IsPlayEnabled = true;
                    this.systemMediaTransportControls.IsPreviousEnabled = this.playQueueService.CanSwitchToPrevious();
                    this.systemMediaTransportControls.IsRecordEnabled = false;
                    this.systemMediaTransportControls.IsRewindEnabled = false;
                    this.systemMediaTransportControls.IsStopEnabled = true;
                });
        }

        private async void OnButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            this.logger.Debug("Pressed {0}", args.Button);
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await this.playQueueService.PlayAsync();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await this.playQueueService.PauseAsync();
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    await this.playQueueService.StopAsync();
                    break;
                case SystemMediaTransportControlsButton.Record:
                    break;
                case SystemMediaTransportControlsButton.FastForward:
                    break;
                case SystemMediaTransportControlsButton.Rewind:
                    break;
                case SystemMediaTransportControlsButton.Next:
                    await this.playQueueService.NextSongAsync();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    await this.playQueueService.PreviousSongAsync();
                    break;
                case SystemMediaTransportControlsButton.ChannelUp:
                    break;
                case SystemMediaTransportControlsButton.ChannelDown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}