// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class CurrentSongPropertiesUpdateService
    {
        private readonly IDispatcher dispatcher;

        private Song currentSong;

        public CurrentSongPropertiesUpdateService(
            IDispatcher dispatcher,
            IGoogleMusicSessionService sessionService,
            IPlayQueueService queueService)
        {
            this.dispatcher = dispatcher;
            
            queueService.StateChanged += this.StateChanged;
            sessionService.SessionCleared += (sender, args) =>
                {
                    this.currentSong = null;
                };
        }

        private async void StateChanged(object sender, StateChangedEventArgs eventArgs)
        {
            await this.dispatcher.RunAsync(
                () =>
                    {
                        if (eventArgs.State == QueueState.Play || eventArgs.State == QueueState.Paused)
                        {
                            if (this.currentSong != eventArgs.CurrentSong && this.currentSong != null)
                            {
                                this.currentSong.State = SongState.None;
                            }

                            this.currentSong = eventArgs.CurrentSong;
                            if (eventArgs.State == QueueState.Play)
                            {
                                this.currentSong.State = SongState.Playing;
                            }
                            else if (eventArgs.State == QueueState.Paused)
                            {
                                this.currentSong.State = SongState.Paused;
                            }
                        }
                        else
                        {
                            if (this.currentSong != null)
                            {
                                this.currentSong.State = SongState.None;
                                this.currentSong = null;
                            }
                        }
                    });
        }
    }
}