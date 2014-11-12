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

        Task<bool?> ShowQuestionAsync(
            string question, 
            Action yesAction = null, 
            Action noAction = null,
            Action cancelAction = null, 
            string yesButton = null, 
            string noButton = null,
            string cancelButton = null);
    }
}