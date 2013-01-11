// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    public interface ILogWriter
    {
        bool IsEnabled { get; }

        void Log(string level, string context, string message, params object[] parameters);
    }
}