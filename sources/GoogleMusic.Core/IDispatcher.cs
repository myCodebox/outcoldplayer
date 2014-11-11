// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Threading.Tasks;

    public enum DispatcherPriority
    {
        Idle = -2,
        Low = -1,
        Normal = 0,
        High = 1
    }

    /// <summary>
    /// The Dispatcher interface.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Run async.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task RunAsync(Action action);

        /// <summary>
        /// Run async.
        /// </summary>
        /// <param name="priority">
        /// The priority.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task RunAsync(DispatcherPriority priority, Action action);
    }
}