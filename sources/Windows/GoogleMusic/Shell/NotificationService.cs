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
        public Task ShowMessageAsync(string message)
        {
            return new MessageDialog(message).ShowAsync().AsTask();
        }
    }
}