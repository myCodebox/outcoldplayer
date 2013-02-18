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
                else if (task.IsCanceled)
                {
                    logger.Warning("Task is cancelled.");
                }
                else if (task.IsCompleted)
                {
                    logger.Debug("Task is completed.");
                }
            }
        }

        public static void LogTask<TResult>(this ILogger logger, Task<TResult> task)
        {
            Debug.Assert(logger != null, "logger != null");
            Debug.Assert(task != null, "task != null");
            if (logger != null && task != null)
            {
                if (task.IsFaulted)
                {
                    logger.LogErrorException(task.Exception);
                }
                else if (task.IsCanceled)
                {
                    logger.Warning("Task is cancelled.");
                }
                else if (task.IsCompleted)
                {
                    logger.Debug("Task is completed. Result {0}.", task.Result);
                }
            }
        }
    }
}