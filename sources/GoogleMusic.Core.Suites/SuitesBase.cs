// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites
{
    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    public abstract class SuitesBase 
    {
        private DependencyResolverContainer container;

        protected IDependencyResolverContainer Container
        {
            get { return this.container; }
        }

        [SetUp]
        public virtual void SetFixture()
        {
            this.container = new DependencyResolverContainer();

            using (var registration = this.container.Registration())
            {
                 registration.Register<ILogManager>().AsSingleton<LogManager>();
            }

            var logManager = this.container.Resolve<ILogManager>();
            logManager.LogLevel = LogLevel.Info;
            logManager.Writers.AddOrUpdate(typeof(DebugLogWriter), type => new DebugLogWriter(), (type, writer) => writer);
        }
    }
}