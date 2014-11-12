// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;
    using System.Threading.Tasks;
    using Windows.System;
    using Windows.UI.Popups;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    public class NotificationService : INotificationService
    {
        private readonly ILogger logger;
        private readonly IDispatcher dispatcher;
        private readonly IApplicationResources resources;

        public NotificationService(IDispatcher dispatcher, ILogManager logManager, IApplicationResources resources)
        {
            this.logger = logManager.CreateLogger("NotificationService");
            this.dispatcher = dispatcher;
            this.resources = resources;
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

        public Task ShowQuestionAsync(string question, Action okAction, Action cancelAction = null, string yesButton = null, string noButton = null)
        {
            var dialog = new MessageDialog(question);

            dialog.Commands.Add(
                new UICommand(
                    string.IsNullOrEmpty(yesButton) ? "Yes" : yesButton,
                    (cmd) =>
                    {
                        if (okAction != null)
                        {
                            okAction();
                        }
                    }));

            dialog.Commands.Add(new UICommand(
                string.IsNullOrEmpty(yesButton) ? "No" : yesButton,
                (cmd) =>
                {
                    if (cancelAction != null)
                    {
                        cancelAction();
                    }
                }));
        

            return dialog.ShowAsync().AsTask();
        }
    }
}