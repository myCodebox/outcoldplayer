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

        public async Task<bool?> ShowQuestionAsync(
            string question, 
            Action yesAction = null, 
            Action noAction = null, 
            Action cancelAction = null, 
            string yesButton = null, 
            string noButton = null, 
            string cancelButton = null)
        {
            var dialog = new MessageDialog(question);

            var yesCommand = new UICommand(
                string.IsNullOrEmpty(yesButton) ? "Yes" : yesButton,
                (cmd) =>
                {
                    if (yesAction != null)
                    {
                        yesAction();
                    }
                });

            var noCommand = new UICommand(
                string.IsNullOrEmpty(yesButton) ? "No" : yesButton,
                (cmd) =>
                {
                    if (noAction != null)
                    {
                        noAction();
                    }
                });

            dialog.Commands.Add(yesCommand);
            dialog.Commands.Add(noCommand);

            UICommand cancelCommand = null;

            if (string.IsNullOrEmpty(cancelButton))
            {
                cancelCommand = new UICommand(
                string.IsNullOrEmpty(cancelButton) ? "Cancel" : cancelButton,
                (cmd) =>
                {
                    if (cancelAction != null)
                    {
                        cancelAction();
                    }
                });

                dialog.Commands.Add(cancelCommand);
            }

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = (uint) (dialog.Commands.Count - 1);

            var resultCommand = await dialog.ShowAsync().AsTask();

            return resultCommand == cancelCommand ? null : (bool?)(resultCommand == yesCommand);
        }
    }
}