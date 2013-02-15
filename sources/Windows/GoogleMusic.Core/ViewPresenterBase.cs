// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    public interface IViewPresenterBase
    {
        void Initialize(IView view);
    }

    public class ViewPresenterBase<TView> : PresenterBase, IViewPresenterBase
        where TView : IView
    {
        public ViewPresenterBase(IDependencyResolverContainer container)
            : base(container)
        {
        }

        public TView View { get; private set; }

        void IViewPresenterBase.Initialize(IView view)
        {
            this.View = (TView)view;
            this.OnInitialized();
        }

        protected virtual void OnInitialized()
        {
        }
    }
}
