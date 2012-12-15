// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System;

    public static class LoggerEx
    {
         public static void LogException(this ILogger @this, Exception exception)
         {
             if (@this == null)
             {
                 throw new ArgumentNullException("@this");
             }

             if (exception == null)
             {
                 throw new ArgumentNullException("exception");
             }

             @this.Error(exception.ToString());
         }
    }
}