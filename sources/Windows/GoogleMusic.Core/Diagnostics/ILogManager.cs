// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    public interface ILogManager
    {
        LogLevel LogLevel { get; set; }

        ILogger CreateLogger(string context);

        void AddWriter(ILogWriter writer);
    }
}