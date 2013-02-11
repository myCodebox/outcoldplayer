// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Collections.Generic;

    public class NavigatingFromEventArgs : EventArgs
    {
        public NavigatingFromEventArgs(IDictionary<string, object> state)
        {
            this.State = state;
        }

        public IDictionary<string, object> State { get; private set; }
    }
}