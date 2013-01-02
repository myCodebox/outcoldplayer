// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System;

    [Flags]
    public enum LogLevel
    {
        None = 0,

        OnlyInfo = 1, 

        Info = OnlyInfo,

        OnlyDebug = 2,

        Debug = OnlyDebug | Info,

        OnlyWarning = 4,

        Warning = OnlyWarning | Debug,

        OnlyError = 8,

        Error = OnlyError | Warning
    }
}