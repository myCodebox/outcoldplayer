// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;

    public static class TaskLogger
    {
         public static void LogTask(this ILogger logger, Task task)
         {
             Debug.Assert(logger != null, "logger != null");
             Debug.Assert(task != null, "task != null");
             if (logger != null && task != null)
             {
                 if (task.IsFaulted)
                 {
                     logger.LogErrorException(task.Exception);
                 }
             }
         }
    }
}