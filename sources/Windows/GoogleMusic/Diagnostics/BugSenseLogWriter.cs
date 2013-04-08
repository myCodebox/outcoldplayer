//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System;

    using OutcoldSolutions.Diagnostics;

    public class BugSenseLogWriter : ILogWriter
    {
        public BugSenseLogWriter()
        {
            this.IsEnabled = true;
        }

        public bool IsEnabled { get; private set; }

        public void Log(DateTime dateTime, LogLevel level, string context, string message, params object[] parameters)
        {
        }

        public void Log(DateTime dateTime, LogLevel level, string context, Exception exception, string messageFormat, params object[] parameters)
        {
            if (level == LogLevel.Warning || level == LogLevel.Error)
            {
                string message;
                if (parameters.Length > 0)
                {
                    message = string.Format(messageFormat, parameters);
                }
                else
                {
                    message = messageFormat;
                }

                BugSense.BugSenseHandler.Instance.LogException(exception, context, message);
            }
        }
    }
}
