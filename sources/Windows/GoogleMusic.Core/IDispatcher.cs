// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Threading.Tasks;

    using Windows.UI.Core;

    public interface IDispatcher
    {
        Task RunAsync(Action action);

        Task RunAsync(CoreDispatcherPriority priority, Action action);
    }
}