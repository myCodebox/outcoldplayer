// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;

    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml.Controls;

    public class PageBase : Page
    {
        private readonly IDependencyResolverContainer container;
#if DEBUG
        private bool presenterInitialized = false;
#endif

        public PageBase()
        {
            this.container = App.Container;
        }

        public virtual void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
#if DEBUG 
            if (!this.presenterInitialized)
            {
                throw new NotSupportedException("View should be initialized with correct presenter!");
            }
#endif

            ((PresenterBase)this.DataContext).OnNavigatedTo(eventArgs);
        }

        public virtual void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
#if DEBUG
            if (!this.presenterInitialized)
            {
                throw new NotSupportedException("View should be initialized with correct presenter!");
            }
#endif

            ((PresenterBase)this.DataContext).OnNavigatingFrom(eventArgs);
        }

        protected void InitializePresenter<TPresenter>() where TPresenter : PresenterBase
        {
#if DEBUG
            this.presenterInitialized = true;
#endif
            this.DataContext = this.container.Resolve<TPresenter>(new object[] { this });
        }

        protected TPresenter Presenter<TPresenter>()
        {
            return (TPresenter)this.DataContext;
        }
    }
}