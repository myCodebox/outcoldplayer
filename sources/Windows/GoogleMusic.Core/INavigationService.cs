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

        IPageView NavigateToView<TViewResolver>(object parameter, bool keepInHistory = true) where TViewResolver : IViewResolver;

        void GoBack();

        bool CanGoBack();

        bool HasHistory();

        void ClearHistory();
    }
}