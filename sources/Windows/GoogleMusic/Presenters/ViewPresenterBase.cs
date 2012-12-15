// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.Views;

    public class ViewPresenterBase<TView> : PresenterBase
        where TView : IView
    {
        public ViewPresenterBase(
            IDependencyResolverContainer container,
            TView view)
            : base(container)
        {
            this.View = view;
        }

        public TView View { get; private set; }
    }
}
