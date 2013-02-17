// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.Diagnostics;

    using Windows.UI.Xaml.Controls;

    public class PageBase : Page, IView
    {
        public PageBase()
        {
        }

        protected ILogger Logger { get; private set; }

        protected IDependencyResolverContainer Container { get; private set; }

        protected PresenterBase Presenter { get; private set; }

        protected TPresenter GetPresenter<TPresenter>() where TPresenter : PresenterBase
        {
            return (TPresenter)this.Presenter;
        }

        [Inject]
        protected void Initialize(
            IDependencyResolverContainer container,
            ILogManager logManager,
            PresenterBase presenterBase)
        {
            this.Container = container;
            this.Presenter = presenterBase;
            this.Logger = this.Container.Resolve<ILogManager>().CreateLogger(this.GetType().Name);
            this.DataContext = presenterBase;

            var viewPresenterBase = presenterBase as IViewPresenterBase;
            if (viewPresenterBase != null)
            {
                viewPresenterBase.Initialize(this);
            }

            this.OnInitialized();
        }

        protected virtual void OnInitialized()
        {
        }
    }
}