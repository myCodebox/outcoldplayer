// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    public interface IPagePresenterBase
    {
        void OnNavigatedTo(NavigatedToEventArgs parameter);

        void OnNavigatingFrom(NavigatingFromEventArgs eventArgs);
    }
}