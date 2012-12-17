// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.GoogleMusic.Views;

    public interface INavigationService
    {
        void NavigateTo<TView>(object parameter = null) where TView : IView;

        void GoBack();

        bool CanGoBack();
    }
}