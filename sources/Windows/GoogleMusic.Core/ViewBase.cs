// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.Diagnostics;

    using Windows.UI.Xaml.Controls;

    public class ViewBase : UserControl, IView
    {
        public ViewBase()
        {
            this.Container = ApplicationBase.Container;
            this.Logger = this.Container.Resolve<ILogManager>().CreateLogger(this.GetType().Name);
        }

        protected internal bool PresenterInitialized { get; private set; }

        protected ILogger Logger { get; private set; }

        protected IDependencyResolverContainer Container { get; private set; }

        protected TPresenter InitializePresenter<TPresenter>() where TPresenter : PresenterBase
        {
            this.PresenterInitialized = true;
            var presenter = this.Container.Resolve<TPresenter>(new object[] { this });
            this.DataContext = presenter;
            return presenter;
        }
    }
}