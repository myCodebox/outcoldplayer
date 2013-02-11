// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;

    public class PageViewBase : ViewBase, IPageView
    {
        public PageViewBase()
        {
            this.NavigationService = this.Container.Resolve<INavigationService>();
        }

        protected INavigationService NavigationService { get; private set; }

        public virtual void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            if (!this.PresenterInitialized)
            {
                throw new NotSupportedException("View should be initialized with correct presenter!");
            }

            ((IPagePresenterBase)this.DataContext).OnNavigatedTo(eventArgs);
        }

        public virtual void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            if (!this.PresenterInitialized)
            {
                throw new NotSupportedException("View should be initialized with correct presenter!");
            }

            ((IPagePresenterBase)this.DataContext).OnNavigatingFrom(eventArgs);
        }
    }
}
