// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    public interface IDataPageView : IPageView
    {
        void OnDataLoading(NavigatedToEventArgs eventArgs);

        void OnDataLoaded(NavigatedToEventArgs eventArgs);
    }
}