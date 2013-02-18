// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    public class PageViewBase : ViewBase, IPageView
    {
        protected INavigationService NavigationService { get; private set; }

        public virtual void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            ((IPagePresenterBase)this.DataContext).OnNavigatedTo(eventArgs);
        }

        public virtual void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            ((IPagePresenterBase)this.DataContext).OnNavigatingFrom(eventArgs);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.NavigationService = this.Container.Resolve<INavigationService>();
        }
    }

    public class DataPageViewBase : PageViewBase, IDataPageView
    {
        public virtual void OnDataLoading()
        {
        }

        public virtual void OnDataLoaded()
        {
        }
    }
}
