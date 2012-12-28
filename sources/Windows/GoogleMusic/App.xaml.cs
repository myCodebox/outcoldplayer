//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.WebServices;

    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.UI.Core;
    using Windows.UI.Xaml;

    public sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        public static IDependencyResolverContainer Container { get; private set; }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainView mainView = Window.Current.Content as MainView;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (mainView == null)
            {
                Container = new DependencyResolverContainer();

                using (var registration = Container.Registration())
                {
                    registration.Register<ILogManager>().As<LogManager>();

                    registration.Register<IMainView>().And<IMediaElemenetContainerView>().As<MainView>();
                    registration.Register<INavigationService>().And<MainViewPresenter>().AsSingleton<MainViewPresenter>();

                    registration.Register<IAuthentificationView>().As<AuthentificationView>();
                    registration.Register<AuthentificationPresenter>();

                    registration.Register<IStartView>().As<StartView>();
                    registration.Register<StartViewPresenter>();

                    registration.Register<IPlaylistsView>().As<PlaylistsView>();
                    registration.Register<PlaylistsViewPresenter>();

                    registration.Register<IPlaylistView>().As<PlaylistView>();
                    registration.Register<PlaylistViewPresenter>();

                    registration.Register<ICurrentPlaylistService>().And<PlayerViewPresenter>().AsSingleton<PlayerViewPresenter>();

                    // Services
                    registration.Register<IClientLoginService>().As<ClientLoginService>();
                    registration.Register<IGoogleWebService>().AsSingleton<GoogleWebService>();
                    registration.Register<IUserDataStorage>().AsSingleton<UserDataStorage>();
                    registration.Register<IAuthentificationService>().As<AuthentificationService>();
                    registration.Register<IPlaylistsWebService>().As<PlaylistsWebService>();
                    registration.Register<ISongWebService>().As<SongWebService>();

                    registration.Register<IDispatcher>().AsSingleton(new DispatcherContainer(CoreWindow.GetForCurrentThread().Dispatcher));
                }

                // Create a Frame to act as the navigation context and navigate to the first page
                mainView = (MainView)Container.Resolve<IMainView>();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = mainView;
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
