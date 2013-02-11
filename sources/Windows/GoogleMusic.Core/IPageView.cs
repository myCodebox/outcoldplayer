// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    public interface IPageView : IView
    {
        void OnNavigatedTo(NavigatedToEventArgs eventArgs);

        void OnNavigatingFrom(NavigatingFromEventArgs eventArgs); 
    }
}