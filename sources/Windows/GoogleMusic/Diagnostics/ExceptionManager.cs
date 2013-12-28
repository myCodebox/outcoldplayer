//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System;

    using BugSense.Model;

    using Windows.UI.Xaml;

    internal class MyExceptionManager : IExceptionManager
    {
        public MyExceptionManager(Application app)
        {
            this.ApplicationContext = app;
            this.ApplicationContext.UnhandledException += this.ApplicationContextOnUnhandledException;
        }

        public event UnhandledExceptionEventHandler UnhandledException;

        public Application ApplicationContext { get; set; }

        private void ApplicationContextOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            if (!unhandledExceptionEventArgs.Handled)
            {
                var handler = UnhandledException;
                if (handler != null)
                {
                    handler(sender, unhandledExceptionEventArgs);
                }
            }
        }
    }
}
