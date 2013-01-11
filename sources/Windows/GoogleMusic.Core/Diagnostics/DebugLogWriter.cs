// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System.Diagnostics;

    public class DebugLogWriter : ILogWriter
    {
        public bool IsEnabled
        {
            get
            {
                return true;
            }
        }

        public void Log(string level, string context, string messageFormat, params object[] parameters)
        {
            string message;

            if (parameters.Length == 0)
            {
                message = messageFormat;
            }
            else
            {
                message = string.Format(messageFormat, parameters);
            }

            Debug.WriteLine("{0} - {1}:: {2}", level, context, message);
        }
    }
}