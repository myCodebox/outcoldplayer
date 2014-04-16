// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System;

    using Windows.UI.Core;

    public class FakeAnalyticsService : IAnalyticsService
    {
        public void SendEvent(string category, string action, string label, int? value = null)
        {   
        }

        public void SendTiming(TimeSpan timeSpan, string category, string action, string label)
        {
        }
    }

    public class AnalyticsService : IAnalyticsService, ILogWriter
    {
        private readonly IDispatcher dispatcher;

        private readonly ILogger logger;

        public AnalyticsService(
            INavigationService navigationService,
            IDispatcher dispatcher,
            ILogManager logManager)
        {
            this.dispatcher = dispatcher;
            this.logger = logManager.CreateLogger("AnalyticsService");
            navigationService.NavigatedTo += NavigationServiceOnNavigatedTo;
        }

        public bool IsEnabled
        {
            get
            {
                return true;
            }
        }

        public void Log(DateTime dateTime, LogLevel level, string context, string messageFormat, params object[] parameters)
        {
        }

        public void Log(
            DateTime dateTime,
            LogLevel level,
            string context,
            Exception exception,
            string messageFormat,
            params object[] parameters)
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
                        GoogleAnalytics.EasyTracker.GetTracker()
                            .SendException(message + " - " + exception.Message, level == LogLevel.Error));

            }
        }

        public void SendEvent(string category, string action, string label, int? value = null)
        {
            try
            {
                this.dispatcher.RunAsync(
                    CoreDispatcherPriority.Low,
                    () =>GoogleAnalytics.EasyTracker.GetTracker().SendEvent(category, action, label, value.HasValue ? value.Value : 0));
            }
            catch (Exception e)
            {
                this.logger.Debug(e, "Could not log navigation");
            }
        }

        public void SendTiming(TimeSpan timeSpan, string category, string action, string label)
        {
            try
            {
                this.dispatcher.RunAsync(
                    CoreDispatcherPriority.Low,
                    () => GoogleAnalytics.EasyTracker.GetTracker().SendTiming(timeSpan, category, action, label));
            }
            catch (Exception e)
            {
                this.logger.Debug(e, "Could not log navigation");
            }
        }

        private void NavigationServiceOnNavigatedTo(object sender, NavigatedToEventArgs eventArgs)
        {
            try
            {
                string viewName = eventArgs.ViewType.Name;
                var request = eventArgs.Parameter as PlaylistNavigationRequest;
                if (request != null)
                {
                    viewName += "-" + request.PlaylistType.ToString("G");
                }

                this.dispatcher.RunAsync(
                    CoreDispatcherPriority.Low,
                    () => GoogleAnalytics.EasyTracker.GetTracker().SendView(viewName));
            }
            catch (Exception e)
            {
                this.logger.Debug(e, "Could not log navigation");
            }
        }
    }
}
