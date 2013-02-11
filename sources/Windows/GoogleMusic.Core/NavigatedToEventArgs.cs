// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Collections.Generic;

    public class NavigatedToEventArgs : EventArgs
    {
        public NavigatedToEventArgs(
            IView view,
            IDictionary<string, object> state, 
            object parameter, 
            bool isBack)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            this.View = view;
            this.State = state;
            this.Parameter = parameter;
            this.IsNavigationBack = isBack;
        }

        public IView View { get; private set; }

        public IDictionary<string, object> State { get; private set; }

        public object Parameter { get; private set; }

        public bool IsNavigationBack { get; private set; }
    }
}