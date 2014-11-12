// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Threading.Tasks;

    public interface INotificationService
    {
        Task ShowMessageAsync(string message);

        Task ShowQuestionAsync(string question, Action okAction, Action cancelAction = null, string yesButton = null, string noButton = null);
    }
}