// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.Media;

    public class MediaControlIntegration : IDisposable
    {
        private readonly IPlayQueueService playQueueService;
        private readonly IDispatcher dispatcher;
        private readonly ILogger logger;

        private bool mediaControlSubscribed = false;
        private bool mediaControlNextSubscribed = false;
        private bool mediaControlPreviousSubscribed = false;

        public MediaControlIntegration(
            ILogManager logManager,
            IPlayQueueService playQueueService,
            IDispatcher dispatcher)
        {
            this.logger = logManager.CreateLogger("MediaControlIntegration");
            this.playQueueService = playQueueService;
            this.dispatcher = dispatcher;
            this.playQueueService.StateChanged += this.StateChanged;
        }

        ~MediaControlIntegration()
        {
            this.Dispose(disposing: false);
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ChangeSubscriptionToMediaControl(subscribe: false);
                this.ChangeSubscriptionToNextTrackMediaControl(subscribe: false);
                this.ChangeSubscriptionToPreviousTrackMediaControl(subscribe: false);
            }
        }

        private void StateChanged(object sender, StateChangedEventArgs eventArgs)
        {
            if (eventArgs.State == QueueState.Play 
                || eventArgs.State == QueueState.Paused 
                || eventArgs.State == QueueState.Busy)
            {
                this.ChangeSubscriptionToMediaControl(subscribe: true);
            }
            else
            {
                this.ChangeSubscriptionToMediaControl(subscribe: false);
            }

            this.ChangeSubscriptionToNextTrackMediaControl(this.playQueueService.CanSwitchToNext());
            this.ChangeSubscriptionToPreviousTrackMediaControl(this.playQueueService.CanSwitchToPrevious());
        }

        private void ChangeSubscriptionToMediaControl(bool subscribe)
        {
            this.dispatcher.RunAsync(() =>
                {
                    if (subscribe)
                    {
                        if (!this.mediaControlSubscribed)
                        {
                            try
                            {
                                MediaControl.PlayPauseTogglePressed += this.MediaControlPlayPauseTogglePressed;
                                MediaControl.PlayPressed += this.MediaControlPlayPressed;
                                MediaControl.PausePressed += this.MediaControlPausePressed;
                                MediaControl.StopPressed += this.MediaControlStopPressed;
                            }
                            catch (Exception e)
                            {
                                this.logger.Debug(e, "Could not subscribe to MediaControl events");
                            }
                            
                            this.mediaControlSubscribed = true;
                        }
                    }
                    else
                    {
                        if (this.mediaControlSubscribed)
                        {
                            try
                            {
                                MediaControl.PlayPauseTogglePressed -= this.MediaControlPlayPauseTogglePressed;
                                MediaControl.PlayPressed -= this.MediaControlPlayPressed;
                                MediaControl.PausePressed -= this.MediaControlPausePressed;
                                MediaControl.StopPressed -= this.MediaControlStopPressed;
                            }
                            catch (Exception e)
                            {
                                this.logger.Debug(e, "Could not subscribe to MediaControl events");
                            }
                            
                            this.mediaControlSubscribed = false;
                        }
                    }
                });
            
        }

        private void ChangeSubscriptionToNextTrackMediaControl(bool subscribe)
        {
            if (subscribe)
            {
                if (!this.mediaControlNextSubscribed)
                {
                    MediaControl.NextTrackPressed += this.MediaControlOnNextTrackPressed;
                    this.mediaControlNextSubscribed = true;
                }
            }
            else
            {
                if (this.mediaControlNextSubscribed)
                {
                    MediaControl.NextTrackPressed -= this.MediaControlOnNextTrackPressed;
                    this.mediaControlNextSubscribed = false;
                }
            }
        }

        private void ChangeSubscriptionToPreviousTrackMediaControl(bool subscribe)
        {
            if (subscribe)
            {
                if (!this.mediaControlPreviousSubscribed)
                {
                    MediaControl.PreviousTrackPressed += this.MediaControlOnPreviousTrackPressed;
                    this.mediaControlPreviousSubscribed = true;
                }
            }
            else
            {
                if (this.mediaControlPreviousSubscribed)
                {
                    MediaControl.PreviousTrackPressed -= this.MediaControlOnPreviousTrackPressed;
                    this.mediaControlPreviousSubscribed = false;
                }
            }
        }

        private async void MediaControlPausePressed(object sender, object e)
        {
            this.logger.Debug("MediaControlPausePressed.");
            await this.playQueueService.PauseAsync();
        }

        private async void MediaControlPlayPressed(object sender, object e)
        {
            this.logger.Debug("MediaControlPlayPressed.");
            await this.playQueueService.PlayAsync();
        }

        private async void MediaControlStopPressed(object sender, object e)
        {
            this.logger.Debug("MediaControlStopPressed.");
            await this.playQueueService.StopAsync();
        }

        private async void MediaControlPlayPauseTogglePressed(object sender, object e)
        {
            this.logger.Debug("MediaControlPlayPauseTogglePressed.");
            if (this.playQueueService.State == QueueState.Play)
            {
                await this.playQueueService.PauseAsync();
            }
            else
            {
                await this.playQueueService.PlayAsync();
            }
        }

        private void MediaControlOnNextTrackPressed(object sender, object o)
        {
            this.logger.Debug("MediaControlOnNextTrackPressed.");
            this.playQueueService.NextSongAsync();
        }

        private void MediaControlOnPreviousTrackPressed(object sender, object o)
        {
            this.logger.Debug("MediaControlOnPreviousTrackPressed.");
            this.playQueueService.PreviousSongAsync();
        }
    }
}