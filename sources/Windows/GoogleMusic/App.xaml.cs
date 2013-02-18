//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
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

    using Windows.UI.Core;
    using Windows.UI.Notifications;
    using Windows.UI.Xaml;

    public sealed partial class App : ApplicationBase
    {
        private ILogManager logManager;
        private ISettingsService settingsService;

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

                    registration.Register<PlaylistViewResolver>();

                    registration.Register<IMainView>()
                                .And<IMediaElemenetContainerView>()
                                .And<ICurrentContextCommands>()
                                .And<IApplicationToolbar>()
                                .InjectionRule<PresenterBase, MainViewPresenter>()
                                .AsSingleton<MainView>();

                    registration.Register<MainViewPresenter>();

                    registration.Register<IAuthentificationView>()
                                .InjectionRule<PresenterBase, AuthentificationPresenter>()
                                .As<AuthentificationPageView>();
                    registration.Register<AuthentificationPresenter>();

                    registration.Register<IStartPageView>()
                                .InjectionRule<PresenterBase, StartPageViewPresenter>()
                                .AsSingleton<StartPageView>();
                    registration.Register<StartPageViewPresenter>().AsSingleton();
                    registration.Register<StartViewBindingModel>().AsSingleton();

                    registration.Register<IPlaylistsPageView>()
                                .InjectionRule<PresenterBase, PlaylistsPageViewPresenter>()
                                .AsSingleton<PlaylistsPageView>();
                    registration.Register<PlaylistsPageViewPresenter>().AsSingleton();
                    registration.Register<PlaylistsPageViewBindingModel>().AsSingleton();

                    registration.Register<IPlaylistPageView>()
                                .InjectionRule<PresenterBase, PlaylistViewPresenter>()
                                .AsSingleton<PlaylistPageView>();
                    registration.Register<PlaylistViewPresenter>().AsSingleton();

                    registration.Register<ICurrentPlaylistView>()
                                .InjectionRule<PresenterBase, CurrentPlaylistViewPresenter>()
                                .AsSingleton<CurrentPlaylistPageView>();
                    registration.Register<CurrentPlaylistViewPresenter>().AsSingleton();

                    registration.Register<IProgressLoadingView>()
                                .InjectionRule<PresenterBase, ProgressLoadingPresenter>()
                                .As<ProgressLoadingPageView>();
                    registration.Register<ProgressLoadingPresenter>();

                    registration.Register<ISearchView>()
                                .InjectionRule<PresenterBase, SearchViewPresenter>()
                                .AsSingleton<SearchPageView>();
                    registration.Register<SearchViewPresenter>();

                    registration.Register<IArtistPageView>()
                                .InjectionRule<PresenterBase, ArtistPageViewPresenter>()
                                .AsSingleton<ArtistPageView>();
                    registration.Register<ArtistPageViewPresenter>().AsSingleton(); 
                    registration.Register<ArtistPageViewBindingModel>().AsSingleton(); 

                    registration.Register<IAlbumPageView>()
                                .InjectionRule<PresenterBase, AlbumPageViewPresenter>()
                                .AsSingleton<AlbumPageView>();
                    registration.Register<AlbumPageViewPresenter>().AsSingleton();
                    registration.Register<AlbumPageViewBindingModel>().AsSingleton();

                    registration.Register<ICurrentPlaylistService>()
                                .And<PlayerViewPresenter>()
                                .AsSingleton<PlayerViewPresenter>();

                    registration.Register<IWhatIsNewView>().As<WhatIsNewView>();

                    registration.Register<IAddToPlaylistPopupView>()
                                .InjectionRule<PresenterBase, AddToPlaylistPopupViewPresenter>()
                                .As<AddToPlaylistPopupView>();
                    registration.Register<AddToPlaylistPopupViewPresenter>();

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

                    registration.Register<ISongMetadataEditService>().AsSingleton<SongMetadataEditService>();
                }

                this.logManager = Container.Resolve<ILogManager>();
                this.settingsService = Container.Resolve<ISettingsService>();

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

                Container.Resolve<IGoogleMusicSessionService>().LoadSession();

                // Place the frame in the current Window
                Window.Current.Content = mainView;

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
            var sessionService = Container.Resolve<IGoogleMusicSessionService>();

            var cookieCollection = Container.Resolve<IGoogleMusicWebService>().GetCurrentCookies();
            if (cookieCollection != null)
            {
                await sessionService.SaveCurrentSessionAsync(cookieCollection);
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
