// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;

    public class ProgressLoadingCloseEventArgs : EventArgs
    {
        public ProgressLoadingCloseEventArgs(bool isFailed)
        {
            this.IsFailed = isFailed;
        }

        public bool IsFailed { get; private set; }
    }
}