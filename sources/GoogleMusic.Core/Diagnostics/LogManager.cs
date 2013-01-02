// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System.Collections.Generic;

    public class LogManager : ILogManager
    {
        private readonly List<ILogWriter> writers = new List<ILogWriter>();

        private LogLevel logLevel;

        public LogLevel LogLevel
        {
            get
            {
                return this.logLevel;
            }

            set
            {
                this.logLevel = value;
                this.UpdateLoggers();
            }
        }

        internal bool IsInfoEnabled
        {
            get { return (this.LogLevel & LogLevel.OnlyInfo) == LogLevel.OnlyInfo; }
        }

        internal bool IsDebugEnabled
        {
            get { return (this.LogLevel & LogLevel.OnlyDebug) == LogLevel.OnlyDebug; }
        }

        internal bool IsWarningEnabled
        {
            get { return (this.LogLevel & LogLevel.OnlyWarning) == LogLevel.OnlyWarning; }
        }

        internal bool IsErrorEnabled
        {
            get { return (this.LogLevel & LogLevel.OnlyError) == LogLevel.OnlyError; }
        }

        public ILogger CreateLogger(string context)
        {
            return new Logger(context, this);
        }

        public void AddWriter(ILogWriter writer)
        {
            this.writers.Add(writer);
            writer.IsEnabled = LogLevel != LogLevel.None;
        }

        internal void Info(string context, string message, params object[] parameters)
        {
            if (this.IsInfoEnabled)
            {
                this.Log("Info", context, message, parameters);
            }
        }

        internal void Debug(string context, string message, params object[] parameters)
        {
            if (this.IsDebugEnabled)
            {
                this.Log("Debug", context, message, parameters);
            }
        }

        internal void Warning(string context, string message, params object[] parameters)
        {
            if (this.IsWarningEnabled)
            {
                this.Log("Warning", context, message, parameters);
            }
        }

        internal void Error(string context, string message, params object[] parameters)
        {
            if (this.IsErrorEnabled)
            {
                this.Log("Error", context, message, parameters);
            }
        }

        private void UpdateLoggers()
        {
            foreach (var logWriter in this.writers)
            {
                logWriter.IsEnabled = LogLevel != LogLevel.None;
            }
        }

        private void Log(string level, string context, string message, params object[] parameters)
        {
            foreach (var logWriter in this.writers)
            {
                try
                {
                    logWriter.Log(level, context, message, parameters);
                }
                catch
                {
                }
            }
        }
    }
}