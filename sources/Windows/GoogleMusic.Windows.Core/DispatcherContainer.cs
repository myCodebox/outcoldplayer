// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Threading.Tasks;

    using Windows.UI.Core;

    /// <summary>
    /// The dispatcher container.
    /// </summary>
    public class DispatcherContainer : IDispatcher
    {
        private readonly CoreDispatcher coreDispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherContainer"/> class.
        /// </summary>
        /// <param name="coreDispatcher">
        /// The core dispatcher.
        /// </param>
        public DispatcherContainer(CoreDispatcher coreDispatcher)
        {
            this.coreDispatcher = coreDispatcher;
        }

        /// <summary>
        /// Run async.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task RunAsync(Action action)
        {
            return this.RunAsync(DispatcherPriority.Normal, action);
        }

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
        public Task RunAsync(DispatcherPriority priority, Action action)
        {
            return this.coreDispatcher.RunAsync((CoreDispatcherPriority)priority, () => action()).AsTask();
        }
    }
}