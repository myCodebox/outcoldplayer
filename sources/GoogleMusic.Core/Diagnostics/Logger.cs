// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    public class Logger : ILogger
    {
        private readonly string context;
        private readonly LogManager logManager;

        public Logger(string context, LogManager logManager)
        {
            this.context = context;
            this.logManager = logManager;
        }

        public bool IsInfoEnabled
        {
            get { return this.logManager.IsInfoEnabled; }
        }

        public bool IsDebugEnabled
        {
            get { return this.logManager.IsDebugEnabled; }
        }

        public bool IsWarningEnabled
        {
            get { return this.logManager.IsWarningEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return this.logManager.IsErrorEnabled; }
        }

        public void Info(string message, params object[] parameters)
        {
            this.logManager.Info(this.context, message, parameters);
        }

        public void Debug(string message, params object[] parameters)
        {
            this.logManager.Debug(this.context, message, parameters);
        }

        public void Warning(string message, params object[] parameters)
        {
            this.logManager.Warning(this.context, message, parameters);
        }

        public void Error(string message, params object[] parameters)
        {
            this.logManager.Error(this.context, message, parameters);
        }
    }
}