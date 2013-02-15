// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    public class PagePresenterBase<TView> : ViewPresenterBase<TView>, IPagePresenterBase
        where TView : IPageView
    {
        public PagePresenterBase(
            IDependencyResolverContainer container)
            : base(container)
        {
        }

        public virtual void OnNavigatedTo(NavigatedToEventArgs parameter)
        {
        }

        public virtual void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
        }
    }
}