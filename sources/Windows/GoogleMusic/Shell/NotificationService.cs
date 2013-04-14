// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;
    using System.Threading.Tasks;

    using Windows.UI.Popups;

    public class NotificationService : INotificationService
    {
        private readonly IDispatcher dispatcher;

        public NotificationService(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public Task ShowMessageAsync(string message)
        {
            return this.dispatcher.RunAsync(async () => { await new MessageDialog(message).ShowAsync().AsTask(); });
        }
    }
}