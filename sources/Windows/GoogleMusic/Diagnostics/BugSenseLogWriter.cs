//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using OutcoldSolutions.Diagnostics;

    using Windows.UI.Core;

    public class BugSenseLogWriter : ILogWriter
    {
        private readonly IDispatcher dispatcher;

        public BugSenseLogWriter(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
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

                this.dispatcher.RunAsync(
                    CoreDispatcherPriority.Low, 
                    () =>
                    {
                        try
                        {
                            var aggregateException = exception as AggregateException;
                            if (aggregateException != null)
                            {
                                exception = aggregateException.Flatten();
                            }

                            var logExtra = new Dictionary<string, string>()
                                               {
                                                   { "level", level.ToString() },
                                                   { "context", context },
                                                   { "message", message }
                                               };

                            if (exception.InnerException != null)
                            {
                                logExtra.Add("exceptionType", exception.GetType().FullName);
                                logExtra.Add("innerExceptionType", exception.InnerException.GetType().FullName);
                                logExtra.Add("innerException", exception.InnerException.Message);
                                logExtra.Add("innerExceptionStackTrace", exception.InnerException.StackTrace);
                            }

                            BugSense.BugSenseHandler.Instance.LogException(
                                exception,
                                logExtra);
                        }
                        catch
                        {
                        }
                    });
                
            }
        }
    }
}
