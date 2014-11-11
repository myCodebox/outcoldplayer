// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    using System;

    public class DonateCloseEventArgs : EventArgs
    {
        public DonateCloseEventArgs(bool later)
        {
            this.Later = later;
        }

        public bool Later { get; set; }
    }
}