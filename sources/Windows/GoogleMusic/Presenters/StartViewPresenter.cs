// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.Views;

    public class StartViewPresenter : ViewPresenterBase<IStartView>
    {
        public StartViewPresenter(
            IDependencyResolverContainer container, 
            IStartView view)
            : base(container, view)
        {
            
        }
    }
}