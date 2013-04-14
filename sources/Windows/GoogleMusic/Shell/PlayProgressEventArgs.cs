// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;

    public class PlayProgressEventArgs : EventArgs
    {
        public PlayProgressEventArgs(
            TimeSpan position,
            TimeSpan duration)
        {
            this.Position = position;
            this.Duration = duration;
        }

        public TimeSpan Position { get; private set; }

        public TimeSpan Duration { get; private set; }
    }
}