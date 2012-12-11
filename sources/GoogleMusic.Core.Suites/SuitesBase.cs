// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites
{
    using OutcoldSolutions.GoogleMusic.Diagnostics;

    using Xunit;

    public abstract class SuitesBase : IUseFixture<DependencyResolverContainer>
    {
        private IDependencyResolverContainer container;

        protected IDependencyResolverContainer Container
        {
            get { return this.container; }
        }

        public virtual void SetFixture(DependencyResolverContainer fixtureContainer)
        {
            fixtureContainer.Behavior.AutoRegistration = true;

            this.container = fixtureContainer;
            using (var registration = this.container.Registration())
            {
                registration.Register<ILogManager>().As<LogManager>();
            }
        }
    }
}