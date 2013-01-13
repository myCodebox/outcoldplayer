// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using System.Collections.Generic;

    public class NavigatedToEventArgs : EventArgs
    {
        public NavigatedToEventArgs(IDictionary<string, object> state, object parameter, bool isBack)
        {
            this.State = state;
            this.Parameter = parameter;
            this.IsBack = isBack;
        }

        public IDictionary<string, object> State { get; private set; }

        public object Parameter { get; private set; }

        public bool IsBack { get; private set; }
    }
}