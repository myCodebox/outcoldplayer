// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.Diagnostics;

    public class PresenterBase : BindingModelBase
    {
        private readonly IDependencyResolverContainer container;

        public PresenterBase(IDependencyResolverContainer container)
        {
            this.container = container;
            this.Logger = this.container.Resolve<ILogManager>().CreateLogger(this.GetType().Name);
            this.Dispatcher = container.Resolve<IDispatcher>();
        }
        
        protected ILogger Logger { get; private set; }

        protected IDispatcher Dispatcher { get; private set; }
    }
}
