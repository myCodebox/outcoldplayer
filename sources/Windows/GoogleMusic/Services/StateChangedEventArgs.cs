// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;

    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public class StateChangedEventArgs : EventArgs
    {
        public StateChangedEventArgs(QueueState state)
        {
            this.State = state;
        }

        public StateChangedEventArgs(QueueState state, Song currentSong)
        {
            this.State = state;
            this.CurrentSong = currentSong;
        }

        public QueueState State { get; private set; }

        public Song CurrentSong { get; private set; }
    }
}