//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Presenters.Popups;
    using OutcoldSolutions.GoogleMusic.Presenters.Settings;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Services.Publishers;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Views.Settings;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Lastfm;

    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.UI.Core;
    using Windows.UI.Notifications;
    using Windows.UI.Xaml;

    public sealed partial class App : Application
    {
        private ILogManager logManager;
        private ISettingsService settingsService;
        private IGoogleMusicSessionService sessionService;
        private IGoogleMusicWebService webService;

        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        public static IDependencyResolverContainer Container { get; private set; }

        protected override void OnSearchActivated(SearchActivatedEventArgs args)
        {
            this.EnsureMainViewActivated();

            base.OnSearchActivated(args);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            this.EnsureMainViewActivated();
        }

        private void EnsureMainViewActivated()
        {
            MainView mainView = Window.Current.Content as MainView;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (mainView == null)
            {
                Container = new DependencyResolverContainer();

                using (var registration = Container.Registration())
                {
                    registration.Register<ILogManager>().AsSingleton<LogManager>();

                    registration.Register<IMainView>()
                                .And<IMediaElemenetContainerView>()
                                .And<ICurrentContextCommands>()
                                .AsSingleton<MainView>();
                    registration.Register<INavigationService>().And<MainViewPresenter>().AsSingleton<MainViewPresenter>();

                    registration.Register<IAuthentificationView>().As<AuthentificationView>();
                    registration.Register<AuthentificationPresenter>();

                    registration.Register<IStartView>().AsSingleton<StartView>();
                    registration.Register<StartViewPresenter>().AsSingleton();

                    registration.Register<IPlaylistsView>().AsSingleton<PlaylistsView>();
                    registration.Register<PlaylistsViewPresenter>().AsSingleton();

                    registration.Register<IPlaylistView>().AsSingleton<PlaylistView>();
                    registration.Register<PlaylistViewPresenter>().AsSingleton();

                    registration.Register<ICurrentPlaylistView>().AsSingleton<CurrentPlaylistView>();
                    registration.Register<CurrentPlaylistViewPresenter>().AsSingleton();

                    registration.Register<IProgressLoadingView>().As<ProgressLoadingView>();
                    registration.Register<ProgressLoadingPresenter>();

                    registration.Register<ISearchView>().AsSingleton<SearchView>();
                    registration.Register<SearchViewPresenter>();

                    registration.Register<ICurrentPlaylistService>()
                                .And<PlayerViewPresenter>()
                                .AsSingleton<PlayerViewPresenter>();

                    registration.Register<IWhatIsNewView>().As<WhatIsNewView>();

                    // Settings
                    registration.Register<ISettingsCommands>().AsSingleton<SettingsCommands>();
                    registration.Register<ISearchService>().AsSingleton<SearchService>();

                    // Settings views
                    registration.Register<AccountView>();
                    registration.Register<AccountViewPresenter>();

                    registration.Register<ILastfmAuthentificationView>().As<LastfmAuthentificationView>();
                    registration.Register<LastfmAuthentificationPresenter>();

                    // Services
                    registration.Register<IDataProtectService>().AsSingleton<DataProtectService>();
                    registration.Register<IGoogleAccountWebService>().As<GoogleAccountWebService>();
                    registration.Register<IGoogleMusicWebService>().AsSingleton<GoogleMusicWebService>();
                    registration.Register<IGoogleAccountService>().AsSingleton<GoogleAccountService>();
                    registration.Register<IAuthentificationService>().As<AuthentificationService>();
                    registration.Register<IPlaylistsWebService>().As<PlaylistsWebService>();
                    registration.Register<ISongWebService>().AsSingleton<SongWebService>();
                    registration.Register<ISettingsService>().AsSingleton<SettingsService>();
                    registration.Register<IGoogleMusicSessionService>().AsSingleton<GoogleMusicSessionService>();

                    registration.Register<IMediaStreamDownloadService>().AsSingleton<MediaStreamDownloadService>();

                    registration.Register<IDispatcher>()
                                .AsSingleton(new DispatcherContainer(CoreWindow.GetForCurrentThread().Dispatcher));

                    registration.Register<ILastfmWebService>().AsSingleton<LastfmWebService>();
                    registration.Register<ILastfmAccountWebService>().As<LastfmAccountWebService>();

                    // Publishers
                    registration.Register<ICurrentSongPublisherService>().AsSingleton<CurrentSongPublisherService>();
                    registration.Register<GoogleMusicCurrentSongPublisher>().AsSingleton();
                    registration.Register<MediaControlCurrentSongPublisher>().AsSingleton();
                    registration.Register<TileCurrentSongPublisher>().AsSingleton();
                    registration.Register<LastFmCurrentSongPublisher>().AsSingleton();

                    // Songs Repositories and Services
                    registration.Register<ISongsRepository>().AsSingleton<SongsRepository>();
                    registration.Register<IMusicPlaylistRepository>().AsSingleton<MusicPlaylistRepository>();
                    registration.Register<IPlaylistCollection<Album>>().AsSingleton<AlbumCollection>();
                    registration.Register<IPlaylistCollection<Artist>>().AsSingleton<ArtistCollection>();
                    registration.Register<IPlaylistCollection<Genre>>().AsSingleton<GenreCollection>();
                    registration.Register<IPlaylistCollection<SystemPlaylist>>().AsSingleton<SystemPlaylistCollection>();
                    registration.Register<IPlaylistCollection<MusicPlaylist>>().AsSingleton<MusicPlaylistCollection>();
                    registration.Register<IPlaylistCollectionsService>().AsSingleton<PlaylistCollectionsService>();
                }

                this.logManager = Container.Resolve<ILogManager>();
                this.settingsService = Container.Resolve<ISettingsService>();
                this.sessionService = Container.Resolve<IGoogleMusicSessionService>();
                this.webService = Container.Resolve<IGoogleMusicWebService>();

                this.UpdateLogLevel();
                this.settingsService.ValueChanged += (sender, eventArgs) =>
                    {
                        if (string.Equals(eventArgs.Key, "IsLoggingOn", StringComparison.OrdinalIgnoreCase))
                        {
                            Task.Factory.StartNew(this.UpdateLogLevel);
                        }
                    };

                // Create a Frame to act as the navigation context and navigate to the first page
                mainView = (MainView)Container.Resolve<IMainView>();

                this.sessionService.LoadSession();

                // Place the frame in the current Window
                Window.Current.Content = mainView;

                // Initialize settings and search views
                Container.Resolve<ISettingsCommands>();
                Container.Resolve<ISearchService>();

                // Publishers
                var currentSongPublisherService = Container.Resolve<ICurrentSongPublisherService>();
                currentSongPublisherService.AddPublisher<GoogleMusicCurrentSongPublisher>();
                currentSongPublisherService.AddPublisher<MediaControlCurrentSongPublisher>();
                currentSongPublisherService.AddPublisher<TileCurrentSongPublisher>();

                if (Container.Resolve<ILastfmWebService>().RestoreSession())
                {
                    currentSongPublisherService.AddPublisher<LastFmCurrentSongPublisher>();
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void UpdateLogLevel()
        {
            var isLoggingOn = this.settingsService.GetValue("IsLoggingOn", defaultValue: false);
            if (isLoggingOn)
            {
                this.logManager.Writers.AddOrUpdate(typeof(FileLogWriter), type => new FileLogWriter(), (type, writer) => writer);
            }
            else
            {
                ILogWriter fileLogWriter;
                if (this.logManager.Writers.TryRemove(typeof(FileLogWriter), out fileLogWriter))
                {
                    ((FileLogWriter)fileLogWriter).Dispose();
                }
            }

            if (Debugger.IsAttached)
            {
                this.logManager.Writers.AddOrUpdate(typeof(DebugLogWriter), type => new DebugLogWriter(), (type, writer) => writer);
            }

            this.logManager.LogLevel = this.logManager.Writers.Count > 0 ? LogLevel.Info : LogLevel.None;
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

            this.OnSuspendingAsync().ContinueWith((t) => deferral.Complete());
        }

        private Task OnSuspendingAsync()
        {
            return Task.Factory.StartNew(
                async () =>
                    {
                        if (this.sessionService != null)
                        {
                            var cookieCollection = this.webService.GetCurrentCookies();
                            if (cookieCollection != null)
                            {
                                await this.sessionService.SaveCurrentSessionAsync(cookieCollection);
                            }
                        }

                        Container.Resolve<ILastfmWebService>().SaveCurrentSession();

                        TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                    });
        }
    }
}
