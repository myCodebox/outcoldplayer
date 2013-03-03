//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using OutcoldSolutions.Controls;
    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
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
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Notifications;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;

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
            using (var registration = Container.Registration())
            {
                registration.Register<IDebugConsole>().AsSingleton<DebugConsole>();

                registration.Register<PlaylistViewResolver>();

                registration.Register<IInitPageView>()
                            .InjectionRule<PresenterBase, InitPageViewPresenter>()
                            .As<InitPageView>();
                registration.Register<InitPageViewPresenter>();

                registration.Register<IPlayerView>()
                            .InjectionRule<PresenterBase, PlayerViewPresenter>()
                            .As<PlayerView>();
                registration.Register<ICurrentPlaylistService>()
                            .And<PlayerViewPresenter>()
                            .AsSingleton<PlayerViewPresenter>();

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
                            .InjectionRule<PresenterBase, PlaylistPageViewPresenter>()
                            .AsSingleton<PlaylistPageView>();
                registration.Register<PlaylistPageViewPresenter>().AsSingleton();
                registration.Register<PlaylistPageViewBindingModel<Playlist>>().AsSingleton();

                registration.Register<ICurrentPlaylistPageView>()
                            .InjectionRule<PresenterBase, CurrentPlaylistPageViewPresenter>()
                            .AsSingleton<CurrentPlaylistPageView>();
                registration.Register<CurrentPlaylistPageViewPresenter>().AsSingleton();
                registration.Register<CurrentPlaylistPageViewBindingModel>().AsSingleton();

                registration.Register<IProgressLoadingView>()
                            .InjectionRule<PresenterBase, ProgressLoadingPresenter>()
                            .As<ProgressLoadingPageView>();
                registration.Register<ProgressLoadingPresenter>();

                registration.Register<ISearchView>()
                            .InjectionRule<PresenterBase, SearchPageViewPresenter>()
                            .AsSingleton<SearchPageView>();
                registration.Register<SearchPageViewPresenter>().AsSingleton();
                registration.Register<SearchPageViewBindingModel>().AsSingleton();

                registration.Register<IArtistPageView>()
                            .InjectionRule<PresenterBase, ArtistPageViewPresenter>()
                            .AsSingleton<ArtistPageView>();
                registration.Register<ArtistPageViewPresenter>().AsSingleton(); 
                registration.Register<ArtistPageViewBindingModel>().AsSingleton(); 

                registration.Register<IAlbumPageView>()
                            .InjectionRule<PresenterBase, AlbumPageViewPresenter>()
                            .AsSingleton<AlbumPageView>();
                registration.Register<AlbumPageViewPresenter>().AsSingleton();
                registration.Register<PlaylistPageViewBindingModel<Album>>().AsSingleton();
                
                registration.Register<IWhatIsNewView>().As<WhatIsNewView>();

                registration.Register<IAddToPlaylistPopupView>()
                            .InjectionRule<PresenterBase, AddToPlaylistPopupViewPresenter>()
                            .As<AddToPlaylistPopupView>();
                registration.Register<AddToPlaylistPopupViewPresenter>();

                // Settings
                registration.Register<ISettingsCommands>().AsSingleton<SettingsCommands>();
                registration.Register<ISearchService>().AsSingleton<SearchService>();

                // Settings views
                registration.Register<AccountPageView>()
                            .InjectionRule<PresenterBase, AccountPageViewPresenter>();
                registration.Register<AccountPageViewPresenter>();

                registration.Register<ILastfmAuthentificationView>()
                            .InjectionRule<PresenterBase, LastfmAuthentificationPresenter>()
                            .As<LastfmAuthentificationPageView>();
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

                registration.Register<RightRegionControlService>().AsSingleton();

                registration.Register<MediaElement>()
                            .AsSingleton(
                                new MediaElement()
                                    {
                                        IsLooping = false,
                                        AutoPlay = true,
                                        AudioCategory = AudioCategory.BackgroundCapableMedia
                                    });
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

            Container.Resolve<IGoogleMusicSessionService>().LoadSession();

            // Publishers
            var currentSongPublisherService = Container.Resolve<ICurrentSongPublisherService>();
            currentSongPublisherService.AddPublisher<GoogleMusicCurrentSongPublisher>();
            currentSongPublisherService.AddPublisher<MediaControlCurrentSongPublisher>();
            currentSongPublisherService.AddPublisher<TileCurrentSongPublisher>();

            if (Container.Resolve<ILastfmWebService>().RestoreSession())
            {
                currentSongPublisherService.AddPublisher<LastFmCurrentSongPublisher>();
            }

            Container.Resolve<ISettingsCommands>().Register();
        }

        protected override void OnActivated()
        {
            Container.Resolve<RightRegionControlService>();

            var mainFrameRegionProvider = Container.Resolve<IMainFrameRegionProvider>();
            mainFrameRegionProvider.SetContent(MainFrameRegion.Links, new LinksRegionContent());
            mainFrameRegionProvider.SetContent(
                MainFrameRegion.Background,
                new Image()
                    {
                        Source = new BitmapImage(new Uri("ms-appx:///Resources/logo460.png")),
                        Height = 460,
                        Width = 460,
                        Margin = new Thickness(20),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Opacity = 0.02
                    });

            var playerView = Container.Resolve<IPlayerView>();
            mainFrameRegionProvider.SetContent(MainFrameRegion.BottomAppBarRightZone, playerView);
            mainFrameRegionProvider.SetContent(
                MainFrameRegion.SnappedView,
                new SnappedPlayerView() { DataContext = playerView.GetPresenter<PlayerViewPresenter>() });

            var page = (Page)Window.Current.Content;
            VisualTreeHelperEx.GetVisualChild<Panel>(page).Children.Add(Container.Resolve<MediaElement>());

            Container.Resolve<IApplicationToolbar>().SetMenuItems(new List<MenuItemMetadata>()
                                                                      {
                                                                          MenuItemMetadata.FromViewType<IStartPageView>("Home"),
                                                                          MenuItemMetadata.FromViewType<ICurrentPlaylistPageView>("Queue"),
                                                                          MenuItemMetadata.FromViewType<IPlaylistsPageView>("Playlists", PlaylistsRequest.Playlists),
                                                                          MenuItemMetadata.FromViewType<IPlaylistsPageView>("Artists", PlaylistsRequest.Artists),
                                                                          MenuItemMetadata.FromViewType<IPlaylistsPageView>("Albums", PlaylistsRequest.Albums),
                                                                          MenuItemMetadata.FromViewType<IPlaylistsPageView>("Genres", PlaylistsRequest.Genres)
                                                                      });

            Container.Resolve<INavigationService>().NavigateTo<IInitPageView>(keepInHistory: false);
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
                this.logManager.Writers.AddOrUpdate(typeof(DebugLogWriter), type => new DebugLogWriter(Container), (type, writer) => writer);
            }

            this.logManager.LogLevel = this.logManager.Writers.Count > 0 ? LogLevel.Info : LogLevel.None;
        }

        private class DebugConsole : IDebugConsole
        {
            public void WriteLine(string message)
            {
                Debug.WriteLine(message);
            }
        }
    }
}
