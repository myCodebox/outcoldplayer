// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml.Controls;

    public class ViewBase : UserControl 
    {
        private readonly IDependencyResolverContainer container;

        public ViewBase(IDependencyResolverContainer container)
        {
            this.container = container;
        }

        protected void InitializePresenter<TPresenter>() where TPresenter : PresenterBase
        {
            this.DataContext = this.container.Resolve<TPresenter>(new object[] { this });
        }

        protected TPresenter Presenter<TPresenter>()
        {
            return (TPresenter)this.DataContext;
        }
    }
}
