// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;

    public class ApplicationLogManager
    {
        private readonly ILogManager logManager;

        private readonly ISettingsService settingsService;

        public ApplicationLogManager(ILogManager logManager, ISettingsService settingsService)
        {
            this.logManager = logManager;
            this.settingsService = settingsService;

            this.UpdateLogLevel();
            settingsService.ValueChanged += (sender, eventArgs) =>
            {
                if (string.Equals(eventArgs.Key, "IsLoggingOn", StringComparison.OrdinalIgnoreCase))
                {
                    Task.Factory.StartNew(this.UpdateLogLevel);
                }
            };
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

            this.logManager.LogLevel = this.logManager.Writers.Count > 0 ? LogLevel.Info : LogLevel.None;
        }
    }
}
