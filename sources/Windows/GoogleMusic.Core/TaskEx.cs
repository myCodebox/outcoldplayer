// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class TaskEx
    {
        public static async Task<Task[]> WhenAllSafe(params Task[] tasks)
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (TaskCanceledException)
            {
            }

            return tasks;
        }

        public static async Task<Task[]> WhenAllSafe(IEnumerable<Task> tasks)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }

            return await WhenAllSafe(tasks.ToArray());
        }
    }
}