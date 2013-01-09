// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites
{
    using NUnit.Framework;

    public abstract class SuitesBase 
    {
        private IDependencyResolverContainer container;

        protected IDependencyResolverContainer Container
        {
            get { return this.container; }
        }

        [SetUp]
        public virtual void SetFixture(DependencyResolverContainer fixtureContainer)
        {
            fixtureContainer.Behavior.AutoRegistration = true;

            this.container = fixtureContainer;
            using (var registration = this.container.Registration())
            {
                // registration.Register<ILogManager>().As<LogManager>();
            }
        }
    }
}