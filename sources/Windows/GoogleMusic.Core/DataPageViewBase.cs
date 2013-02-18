// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    public class DataPageViewBase : PageViewBase, IDataPageView
    {
        public virtual void OnDataLoading(NavigatedToEventArgs eventArgs)
        {
        }

        public virtual void OnUnfreeze(NavigatedToEventArgs eventArgs)
        {
        }

        public virtual void OnDataLoaded(NavigatedToEventArgs eventArgs)
        {
        }
    }
}