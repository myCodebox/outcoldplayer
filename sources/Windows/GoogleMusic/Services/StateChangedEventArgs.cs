// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class StateChangedEventArgs : EventArgs
    {
        public StateChangedEventArgs(QueueState state)
        {
            this.State = state;
        }

        public StateChangedEventArgs(QueueState state, SongBindingModel currentSong)
        {
            this.State = state;
            this.CurrentSong = currentSong;
        }

        public QueueState State { get; private set; }

        public SongBindingModel CurrentSong { get; private set; }
    }
}