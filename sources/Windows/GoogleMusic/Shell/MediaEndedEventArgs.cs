// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;

    public class MediaEndedEventArgs : EventArgs
    {
        public MediaEndedEventArgs(MediaEndedReason reason)
        {
            this.Reason = reason;
        }

        public MediaEndedReason Reason { get; private set; }
    }
}