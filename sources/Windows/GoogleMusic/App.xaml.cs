//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
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
    
    using Windows.UI.Core;
    using Windows.UI.Notifications;
    using Windows.UI.Xaml;

    using DebugLogWriter = OutcoldSolutions.GoogleMusic.Diagnostics.DebugLogWriter;

    public sealed partial class App : ApplicationBase
    {
        private ILogManager logManager;
        private ISettingsService settingsService;
        private IGoogleMusicSessionService sessionService;
        private IGoogleMusicWebService webService;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void InitializeApplication()
        {
            MainView mainView = Window.Current.Content as MainView;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (mainView == null)
            {
                using (var registration = Container.Registration())
                {
                    registration.Register<ILogManager>().AsSingleton<LogManager>();

                    registration.Register<INavigationService>().AsSingleton<NavigationService>();

                    registration.Register<IMainView>()
                                .And<IMediaElemenetContainerView>()
                                .And<ICurrentContextCommands>()
                                .AsSingleton<MainView>();

                    registration.Register<MainViewPresenter>();

                    registration.Register<IAuthentificationView>().As<AuthentificationPageView>();
                    registration.Register<AuthentificationPresenter>();

                    registration.Register<IStartView>().AsSingleton<StartPageView>();
                    registration.Register<StartViewPresenter>().AsSingleton();

                    registration.Register<IPlaylistsView>().AsSingleton<PlaylistsPageView>();
                    registration.Register<PlaylistsViewPresenter>().AsSingleton();

                    registration.Register<IPlaylistView>().AsSingleton<PlaylistPageView>();
                    registration.Register<PlaylistViewPresenter>().AsSingleton();

                    registration.Register<ICurrentPlaylistView>().AsSingleton<CurrentPlaylistPageView>();
                    registration.Register<CurrentPlaylistViewPresenter>().AsSingleton();

                    registration.Register<IProgressLoadingView>().As<ProgressLoadingPageView>();
                    registration.Register<ProgressLoadingPresenter>();

                    registration.Register<ISearchView>().AsSingleton<SearchPageView>();
                    registration.Register<SearchViewPresenter>();

                    registration.Register<ICurrentPlaylistService>()
                                .And<PlayerViewPresenter>()
                                .AsSingleton<PlayerViewPresenter>();

                    registration.Register<IWhatIsNewView>().As<WhatIsNewView>();

                    // Settings
                    registration.Register<ISettingsCommands>().AsSingleton<SettingsCommands>();
                    registration.Register<ISearchService>().AsSingleton<SearchService>();

                    // Settings views
                    registration.Register<AccountPageView>();
                    registration.Register<AccountViewPresenter>();

                    registration.Register<ILastfmAuthentificationView>().As<LastfmAuthentificationPageView>();
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

                    registration.Register<ISongsCacheService>().AsSingleton<SongsCacheService>();
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

                Container.Resolve<INavigationService>().RegisterRegionProvider(mainView);

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

        protected override async Task OnSuspendingAsync()
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
            await Container.Resolve<ISongsRepository>().SaveToCacheAsync();

            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
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
    }
}
