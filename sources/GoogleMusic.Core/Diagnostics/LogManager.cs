// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    public class LogManager : ILogManager
    {
        public ILogger CreateLogger(string context)
        {
            return new Logger(context);
        }
    }
}