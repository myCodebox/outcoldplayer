// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Threading.Tasks;

    using Windows.UI.Core;

    public class DispatcherContainer : IDispatcher
    {
        private readonly CoreDispatcher coreDispatcher;

        public DispatcherContainer(CoreDispatcher coreDispatcher)
        {
            this.coreDispatcher = coreDispatcher;
        }

        public Task RunAsync(Action action)
        {
            return this.RunAsync(CoreDispatcherPriority.Normal, action);
        }

        public Task RunAsync(CoreDispatcherPriority priority, Action action)
        {
            return this.coreDispatcher.RunAsync(priority, () => action()).AsTask();
        }
    }
}