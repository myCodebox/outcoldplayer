// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;

    public interface INavigationService
    {
        event EventHandler<NavigatedToEventArgs> NavigatedTo;

        void RegisterRegionProvider(IViewRegionProvider regionProvider);

        TView NavigateTo<TView>(object parameter = null, bool keepInHistory = true) where TView : IPageView;

        void GoBack();

        bool CanGoBack();

        bool HasHistory();

        void ClearHistory();
    }
}