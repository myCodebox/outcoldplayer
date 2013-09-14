// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;

    using Windows.UI.Popups;

    public class NotificationService : INotificationService
    {
        private readonly ILogger logger;
        private readonly IDispatcher dispatcher;

        public NotificationService(IDispatcher dispatcher, ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("NotificationService");
            this.dispatcher = dispatcher;
        }

        public Task ShowMessageAsync(string message)
        {
            return this.dispatcher.RunAsync(
                async () =>
                    {
                        {
                            try
                            {
                                await new MessageDialog(message).ShowAsync().AsTask();
                            }
                            catch (Exception e)
                            {
                                this.logger.Error(e, "ShowMessageAsync failed");
                            }
                        }
                    });
        }
    }
}