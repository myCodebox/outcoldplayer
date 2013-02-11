// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System.Threading.Tasks;

    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.UI.Xaml;

    public abstract class ApplicationBase : Application
    {
        protected ApplicationBase()
        {
            this.Suspending += this.OnSuspending;
        }

        public static IDependencyResolverContainer Container { get; private set; }

        protected override void OnSearchActivated(SearchActivatedEventArgs args)
        {
            this.InitializeInternal();

            base.OnSearchActivated(args);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            this.InitializeInternal();

            base.OnLaunched(args);
        }

        protected abstract void InitializeApplication();

        protected abstract Task OnSuspendingAsync();

        private void InitializeInternal()
        {
            if (Container == null)
            {
                Container = new DependencyResolverContainer();
                this.InitializeApplication();
            }
        }

        private void OnSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        {
            var deferral = suspendingEventArgs.SuspendingOperation.GetDeferral();

            var suspendingTask = this.OnSuspendingAsync();
            if (suspendingTask != null)
            {
                suspendingTask.ContinueWith((t) => deferral.Complete());
            }
            else
            {
                deferral.Complete();
            }
        }
    }
}