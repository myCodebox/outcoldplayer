// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System;

    public interface ILogWriter
    {
        bool IsEnabled { get; }

        void Log(DateTime dateTime, string level, string context, string message, params object[] parameters);
    }
}