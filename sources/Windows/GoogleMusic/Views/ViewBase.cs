// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml.Controls;

    public class ViewBase : UserControl, IView
    {
        private readonly IDependencyResolverContainer container;
#if DEBUG
        private bool presenterInitialized = false;
#endif

        public ViewBase()
        {
            this.container = App.Container;
            this.Logger = this.container.Resolve<ILogManager>().CreateLogger(this.GetType().Name);
        }

        protected ILogger Logger { get; private set; }

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
