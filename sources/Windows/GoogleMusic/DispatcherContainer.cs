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

        public async Task RunAsync(Action action)
        {
            await this.coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }
    }
}