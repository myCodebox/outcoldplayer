//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reactive.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Windows.ApplicationModel.Activation;
    using Windows.Storage;
    using Windows.UI.Core;

    using BugSense;
    using BugSense.Core.Model;
    using BugSense.Model;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.EventAggregator;
    using OutcoldSolutions.GoogleMusic.InversionOfControl;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Presenters.Popups;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Services.Actions;
    using OutcoldSolutions.GoogleMusic.Services.Publishers;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Lastfm;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;

    using Windows.ApplicationModel;
    using Windows.UI.Notifications;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;

    public sealed partial class App : ApplicationBase
    {
        public App()
        {
            this.InitializeComponent();

            this.Suspending += this.OnSuspending;
#if !DEBUG
            BugSense.BugSenseHandler.Instance.InitAndStartSession(new ExceptionManager(this), "w8c8d6b5");
#endif
        }

        /// <summary>
        /// The on launched.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            this.InitializeInternal();

            base.OnLaunched(args);

            // Navigate and play from tiles
            if (!string.IsNullOrEmpty(args.Arguments))
            {
                await Task.Run(async () =>
                {
                    int indexOf = args.Arguments.IndexOf('_');
                    PlaylistType playlistType =
                        (PlaylistType) Enum.Parse(typeof (PlaylistType), args.Arguments.Substring(0, indexOf));
                    string playlistId = args.Arguments.Substring(indexOf + 1);
                    IPlaylist playlist = await ApplicationContext.Container.Resolve<IPlaylistsService>().GetAsync(playlistType, playlistId);
                    if (playlist != null)
                    {
                        await ApplicationContext.Container.Resolve<IDispatcher>().RunAsync(() =>
                        {
                            ApplicationContext.Container.Resolve<INavigationService>()
                                .NavigateToPlaylist(new PlaylistNavigationRequest(playlist) {ForceToPlay = true});
                        });
        }
                });
            }
        }

        private void InitializeInternal()
        {
            bool isFirstTimeActivate = false;

            MainFrame mainFrame = Window.Current.Content as MainFrame;
            if (mainFrame == null)
            {
                isFirstTimeActivate = true;

                if (ApplicationContext.ApplicationLocalFolder == null)
                {
                    ApplicationContext.ApplicationLocalFolder = new WindowsStorageFolder(ApplicationData.Current.LocalFolder);
                }

                if (ApplicationContext.MusicLibraryFolder == null)
                {
                    ApplicationContext.MusicLibraryFolder = new WindowsStorageFolder(KnownFolders.MusicLibrary);
                }

                if (ApplicationContext.Container == null)
                {
                    ApplicationContext.ApplicationVersion = new Version(
                        Package.Current.Id.Version.Major,
                        Package.Current.Id.Version.Minor, 
                        Package.Current.Id.Version.Build,
                        Package.Current.Id.Version.Revision);

                    ApplicationContext.Container = new DependencyResolverContainer();

                    using (var registration = ApplicationContext.Container.Registration())
                    {
                        registration.Register<IEventAggregator>().AsSingleton<EventAggregator.EventAggregator>();
                        registration.Register<ILogManager>().AsSingleton<LogManager>();
                        registration.Register<INavigationService>().AsSingleton<NavigationService>();
                        registration.Register<IDispatcher>().AsSingleton(new DispatcherContainer(CoreWindow.GetForCurrentThread().Dispatcher));
                        registration.Register<ITimer>().As<DispatcherTimerWrapper>();
                        registration.Register<IShellService>().As<ShellService>();

                        registration.Register<IMainFrame>()
                                    .And<IMainFrameRegionProvider>()
                                    .InjectionRule<BindingModelBase, MainFramePresenter>()
                                    .AsSingleton<MainFrame>();
                        registration.Register<MainFramePresenter>().AsSingleton();

                        registration.Register<IApplicationSettingFrame>()
                                    .InjectionRule<BindingModelBase, ApplicationSettingFramePresenter>()
                                    .As<ApplicationSettingFrame>();
                        registration.Register<ApplicationSettingFramePresenter>();

                        registration.Register<IApplicationSettingViewsService>()
                                    .AsSingleton<ApplicationSettingViewsService>();

                        registration.Register<IInAppPurchasesService>()
                                    .AsSingleton<InAppPurchasesService>();
                    }

                    this.Logger = ApplicationContext.Container.Resolve<ILogManager>().CreateLogger(this.GetType().Name);

                    this.InitializeApplication();
                }

                mainFrame = (MainFrame)ApplicationContext.Container.Resolve<IMainFrame>();
                ((IViewPresenterBase)ApplicationContext.Container.Resolve<MainFramePresenter>()).Initialize(mainFrame);
                ApplicationContext.Container.Resolve<INavigationService>().RegisterRegionProvider(mainFrame);
                Window.Current.Content = mainFrame;
            }

            Window.Current.Activate();

            this.OnActivated(isFirstTimeActivate);
        }

        private void InitializeApplication()
        {
            this.Resources["ApplicationName"] = string.Format(CultureInfo.CurrentCulture, "outcoldplayer {0}", ApplicationContext.ApplicationVersion.ToVersionString());
            this.Resources["ApplicationVersion"] = ApplicationContext.ApplicationVersion.ToVersionString();

            using (var registration = ApplicationContext.Container.Registration())
            {
#if DEBUG
                registration.Register<IDebugConsole>().AsSingleton<DebugConsole>();
#endif

                Registration.RegisterPages(registration);
                Registration.RegisterSettingViews(registration);
                Registration.RegisterPopupViews(registration);
                Registration.RegisterViews(registration);

                registration.Register<IApplicationResources>().AsSingleton<ApplicationResources>();

                // Settings
                registration.Register<ILastfmAuthentificationView>()
                            .InjectionRule<BindingModelBase, LastfmAuthentificationPresenter>()
                            .As<LastfmAuthentificationPageView>();
                registration.Register<LastfmAuthentificationPresenter>();

                // Services
                registration.Register<IDataProtectService>().AsSingleton<DataProtectService>();
                registration.Register<IGoogleAccountWebService>().As<GoogleAccountWebService>();
                registration.Register<IGoogleMusicWebService>().AsSingleton<GoogleMusicWebService>();
                registration.Register<IGoogleMusicApisService>().AsSingleton<GoogleMusicApisService>();
                registration.Register<IGoogleAccountService>().AsSingleton<GoogleAccountService>();
                registration.Register<IAuthentificationService>().As<AuthentificationService>();
                registration.Register<IPlaylistsWebService>().As<PlaylistsWebService>();
                registration.Register<ISongsWebService>().AsSingleton<SongsWebService>();
                registration.Register<IRadioWebService>().AsSingleton<RadioWebService>();
                registration.Register<IAllAccessWebService>().AsSingleton<AllAccessWebService>();
                registration.Register<IAllAccessService>().AsSingleton<AllAccessService>();
                registration.Register<ISettingsService>().AsSingleton<SettingsService>();
                registration.Register<IGoogleMusicSessionService>().AsSingleton<GoogleMusicSessionService>();
                registration.Register<IConfigWebService>().AsSingleton<ConfigWebService>();

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
                registration.Register<ICachedSongsRepository>().AsSingleton<CachedSongsRepository>();
                registration.Register<ISongsRepository>().AsSingleton<SongsRepository>();
                registration.Register<IUserPlaylistsRepository>().And<IPlaylistRepository<UserPlaylist>>().AsSingleton<UserPlaylistsRepository>();
                registration.Register<IArtistsRepository>().And<IPlaylistRepository<Artist>>().AsSingleton<ArtistsRepository>();
                registration.Register<IAlbumsRepository>().And<IPlaylistRepository<Album>>().AsSingleton<AlbumsRepository>();
                registration.Register<IGenresRepository>().And<IPlaylistRepository<Genre>>().AsSingleton<GenresRepository>();
                registration.Register<ISystemPlaylistsRepository>().And<IPlaylistRepository<SystemPlaylist>>().AsSingleton<SystemPlaylistsRepository>();
                registration.Register<IPlaylistsService>().AsSingleton<PlaylistsService>();
                registration.Register<IUserPlaylistsService>().AsSingleton<UserPlaylistsService>();
                registration.Register<IAlbumArtCacheService>().AsSingleton<AlbumArtCacheService>();
                registration.Register<ICachedAlbumArtsRepository>().AsSingleton<CachedAlbumArtsRepository>();
                registration.Register<IRadioStationsRepository>().And<IPlaylistRepository<Radio>>().AsSingleton<RadioStationsRepository>();
                registration.Register<IRadioStationsService>().AsSingleton<RadioStationsService>();

                registration.Register<IInitialSynchronization>().As<InitialSynchronization>();

                registration.Register<ISongsService>().AsSingleton<SongsService>();

                registration.Register<ApplicationLogManager>().AsSingleton();

                registration.Register<ISongsCachingService>().AsSingleton<SongsCachingService>();

                registration.Register<IApplicationStateService>().AsSingleton<ApplicationStateService>();

                registration.Register<MediaElement>()
                            .AsSingleton(
                                new MediaElement()
                                    {
                                        IsLooping = false,
                                        AutoPlay = true,
                                        AudioCategory = AudioCategory.BackgroundCapableMedia
                                    });

                registration.Register<IMediaElementContainer>()
                            .AsSingleton<MediaElementContainer>();

                registration.Register<IPlayQueueService>()
                            .AsSingleton<PlayQueueService>();

                registration.Register<INotificationService>()
                            .AsSingleton<NotificationService>();

                registration.Register<IMediaControlIntegration>()
                            .AsSingleton<MediaControlIntegration>();

                registration.Register<IGoogleMusicSynchronizationService>()
                            .AsSingleton<GoogleMusicSynchronizationService>();

                registration.Register<ScreenLocker>();
                registration.Register<ApplicationStateChangeHandler>();

                // Actions
                registration.Register<ISelectedObjectsService>()
                            .AsSingleton<SelectedObjectsService>();

                registration.Register<QueueAction>().AsSingleton();
                registration.Register<StartRadioAction>().AsSingleton();
                registration.Register<AddToPlaylistAction>().AsSingleton();
                registration.Register<EditPlaylistAction>().AsSingleton();
                registration.Register<DownloadAction>().AsSingleton();
                registration.Register<RemoveLocalAction>().AsSingleton();
                registration.Register<RemoveFromPlaylistAction>().AsSingleton();
                registration.Register<RemoveSelectedSongAction>().AsSingleton();
                registration.Register<DeletePlaylistAction>().AsSingleton();
                registration.Register<DeleteRadioStationsAction>().AsSingleton();
                registration.Register<AddToLibraryAction>().AsSingleton();
                registration.Register<RemoveFromLibraryAction>().AsSingleton();
                registration.Register<PinToStartAction>().AsSingleton();

                registration.Register<ApplicationSize>().AsSingleton(this.Resources["ApplicationSize"]);

                registration.Register<AskForReviewService>().AsSingleton();

                registration.Register<IRatingCacheService>().AsSingleton<RatingCacheService>();

#if DEBUG
                registration.Register<IAnalyticsService>().AsSingleton<FakeAnalyticsService>();
#else
                registration.Register<IAnalyticsService>().AsSingleton<AnalyticsService>();
#endif
            }

            ApplicationContext.Container.Resolve<ApplicationLogManager>();

            ApplicationContext.Container.Resolve<IGoogleMusicSessionService>().LoadSession();

            // Publishers
            var currentSongPublisherService = ApplicationContext.Container.Resolve<ICurrentSongPublisherService>();
            currentSongPublisherService.AddPublisher<GoogleMusicCurrentSongPublisher>();
            currentSongPublisherService.AddPublisher<MediaControlCurrentSongPublisher>();
            currentSongPublisherService.AddPublisher<TileCurrentSongPublisher>();

            var selectedObjectsService = ApplicationContext.Container.Resolve<ISelectedObjectsService>();
            selectedObjectsService.AddActions(new ISelectedObjectAction []
                                              {
                                                  ApplicationContext.Container.Resolve<QueueAction>(),
                                                  ApplicationContext.Container.Resolve<StartRadioAction>(),
                                                  ApplicationContext.Container.Resolve<AddToPlaylistAction>(),
                                                  ApplicationContext.Container.Resolve<EditPlaylistAction>(),
                                                  ApplicationContext.Container.Resolve<AddToLibraryAction>(),
                                                  ApplicationContext.Container.Resolve<DownloadAction>(),
                                                  ApplicationContext.Container.Resolve<RemoveLocalAction>(),
                                                  ApplicationContext.Container.Resolve<RemoveFromPlaylistAction>(),
                                                  ApplicationContext.Container.Resolve<RemoveFromLibraryAction>(),
                                                  ApplicationContext.Container.Resolve<RemoveSelectedSongAction>(),
                                                  ApplicationContext.Container.Resolve<DeletePlaylistAction>(),
                                                  ApplicationContext.Container.Resolve<DeleteRadioStationsAction>(),
                                                  ApplicationContext.Container.Resolve<PinToStartAction>()
                                              });

            if (ApplicationContext.Container.Resolve<ILastfmWebService>().RestoreSession())
            {
                currentSongPublisherService.AddPublisher<LastFmCurrentSongPublisher>();
            }
        }

        private void OnActivated(bool isFirstTimeActivated)
        {
            if (isFirstTimeActivated)
            {
                ApplicationContext.Container.Resolve<ApplicationStateChangeHandler>();

                var mainFrameRegionProvider = ApplicationContext.Container.Resolve<IMainFrameRegionProvider>();
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

                mainFrameRegionProvider.SetContent(MainFrameRegion.BottomAppBarRightZone, ApplicationContext.Container.Resolve<IPlayerView>());

                var page = (Page)Window.Current.Content;
                VisualTreeHelperEx.GetVisualChild<Panel>(page).Children.Add(ApplicationContext.Container.Resolve<MediaElement>());

                ApplicationSettingViews.Initialize(ApplicationContext.Container.Resolve<IApplicationSettingViewsService>(), ApplicationContext.Container.Resolve<IApplicationResources>());

                ApplicationContext.Container.Resolve<IMediaControlIntegration>();
                ApplicationContext.Container.Resolve<ScreenLocker>();

                ApplicationContext.Container.Resolve<INavigationService>().NavigateTo<IHomePageView>();

                this.ReportOsVersionAsync();

                ApplicationContext.Container.Resolve<IEventAggregator>().GetEvent<ApplicationInitializedEvent>().Subscribe(
                    async (e) =>
                    {
                        var dispatcher = ApplicationContext.Container.Resolve<IDispatcher>();
                        await dispatcher.RunAsync(() => ApplicationContext.Container.Resolve<IMainFrameRegionProvider>().SetContent(MainFrameRegion.Links, ApplicationContext.Container.Resolve<LinksRegionView>()));

                        ApplicationContext.Container.Resolve<ISongsCachingService>().StartDownloadTask();
                        ApplicationContext.Container.Resolve<AskForReviewService>();
                    });

                this.UpdateRatingControlStyle();

                ApplicationContext.Container.Resolve<IEventAggregator>().GetEvent<SettingsChangeEvent>()
                    .Where(x => string.Equals(x.Key, SettingsServiceExtensions.IsThumbsRatingKey))
                    .Subscribe(async (x) => await ApplicationContext.Container.Resolve<IDispatcher>().RunAsync(this.UpdateRatingControlStyle));
            }
        }

        private void UpdateRatingControlStyle()
        {
            this.Resources.Remove(typeof(Rating));
            if (ApplicationContext.Container.Resolve<ISettingsService>().GetIsThumbsRating())
            {
                this.Resources.Add(typeof(Rating), this.Resources["ThumbsRatingStyle"]);
            }
            else
            {
                this.Resources.Add(typeof(Rating), this.Resources["5StarRatingStyle"]);
            }
        }

        private void ReportOsVersionAsync()
        {
            try
            {
                ApplicationContext.Container.Resolve<IAnalyticsService>().SendEvent("Application", "Build", "Windows 8.1");
            }
            catch (Exception e)
            {
                this.Logger.Debug(e, "Cannot report os version");
            }
        }

        private async Task OnSuspendingAsync()
        {
            await ApplicationContext.Container.Resolve<IGoogleMusicSessionService>().SaveCurrentSessionAsync();

            ApplicationContext.Container.Resolve<ILastfmWebService>().SaveCurrentSession();

            await GoogleAnalytics.EasyTracker.Current.Dispatch().AsTask();

            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
        }

        private void OnSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        {
            var suspendingTask = this.OnSuspendingAsync();
            
            var deferral = suspendingEventArgs.SuspendingOperation.GetDeferral();

            suspendingTask.ContinueWith((t) =>
            {
                if (this.Logger != null)
                {
                    this.Logger.LogTask(t);
                }

                deferral.Complete();
            });        
        }
    }
}
