// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System.Threading.Tasks;

    public interface INotificationService
    {
        Task ShowMessageAsync(string message);
    }
}