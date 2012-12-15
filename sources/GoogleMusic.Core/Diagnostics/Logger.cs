// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    public class Logger : ILogger
    {
        private readonly string context;

        public Logger(string context)
        {
            this.context = context;
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

        public bool IsDebugEnabled
        {
            get { return true; }
        }

        public bool IsWarningEnabled
        {
            get { return true; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public void Info(string message, params object[] parameters)
        {
            System.Diagnostics.Debug.WriteLine(this.context + " - " + message, parameters);
        }

        public void Debug(string message, params object[] parameters)
        {
            System.Diagnostics.Debug.WriteLine(this.context + " - " + message, parameters);
        }

        public void Warning(string message, params object[] parameters)
        {
            System.Diagnostics.Debug.WriteLine(this.context + " - " + message, parameters);
        }

        public void Error(string message, params object[] parameters)
        {
            System.Diagnostics.Debug.WriteLine(this.context + " - " + message, parameters);
        }
    }
}