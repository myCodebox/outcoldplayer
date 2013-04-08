// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Diagnostics;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.UI.Xaml;

    public class ApplicationLogManager
    {
        private readonly ILogManager logManager;

        private readonly ISettingsService settingsService;

        public ApplicationLogManager(ILogManager logManager, ISettingsService settingsService, IEventAggregator eventAggregator)
        {
            Application.Current.UnhandledException += this.CurrentOnUnhandledException;

            this.logManager = logManager;
            this.settingsService = settingsService;

            eventAggregator.GetEvent<SettingsChangeEvent>()
                           .Where(e => string.Equals(e.Key, "IsLoggingOn", StringComparison.OrdinalIgnoreCase))
                           .Subscribe(e => Task.Factory.StartNew(this.UpdateLogLevel));

            this.UpdateLogLevel();
        }

        private void CurrentOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var logger = this.logManager.CreateLogger("App");
            logger.Error(unhandledExceptionEventArgs.Exception, "Unhandled exception: {0}.", unhandledExceptionEventArgs.Message);

            Debug.Assert(false, unhandledExceptionEventArgs.Message);
        }

        private void UpdateLogLevel()
        {
            var isLoggingOn = this.settingsService.GetValue("IsLoggingOn", defaultValue: false);
            if (isLoggingOn)
            {
                this.logManager.Writers.AddOrUpdate(typeof(FileLogWriter), type => new FileLogWriter(), (type, writer) => writer);
            }
            else
            {
                ILogWriter fileLogWriter;
                if (this.logManager.Writers.TryRemove(typeof(FileLogWriter), out fileLogWriter))
                {
                    ((FileLogWriter)fileLogWriter).Dispose();
                }
            }

            if (Debugger.IsAttached)
            {
                this.logManager.Writers.AddOrUpdate(typeof(DebugLogWriter), type => new DebugLogWriter(ApplicationBase.Container), (type, writer) => writer);
            }
            
#if !DEBUG
            this.logManager.Writers.AddOrUpdate(typeof(BugSenseLogWriter), type => new BugSenseLogWriter(), (type, writer) => writer);
#endif

            this.logManager.LogLevel = this.logManager.Writers.Count > 1 ? LogLevel.Info : LogLevel.Warning;
        }
    }
}
