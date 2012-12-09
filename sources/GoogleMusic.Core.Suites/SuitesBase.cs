// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites
{
    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    public abstract class SuitesBase
    {
        private IDependencyResolverContainer container;

        protected IDependencyResolverContainer Container
        {
            get { return this.container; }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            this.container = new DependencyResolverContainer();
            using (var registration = this.container.Registration())
            {
                registration.Register<ILogManager>().As<LogManager>();
            }

            this.OnTestFixtureSetUp();
        }

        public virtual void OnTestFixtureSetUp()
        {
        }
    }
}