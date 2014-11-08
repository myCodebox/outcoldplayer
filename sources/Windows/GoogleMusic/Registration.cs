//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.BindingModels.Settings;
    using OutcoldSolutions.GoogleMusic.InversionOfControl;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Presenters.Popups;
    using OutcoldSolutions.GoogleMusic.Presenters.Settings;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Views.Settings;

    internal static class Registration
    {
        public static void RegisterPages(IRegistrationContext registration)
        {
            // Start page view
            registration.Register<IHomePageView>()
                        .InjectionRule<BindingModelBase, HomePageViewPresenter>()
                        .AsSingleton<HomePageView>();
            registration.Register<HomePageViewPresenter>().AsSingleton();
            registration.Register<HomePageViewBindingModel>();

            // Playlists view (albums, genres, artists)
            registration.Register<IPlaylistsPageView>()
                        .InjectionRule<BindingModelBase, PlaylistsPageViewPresenter>()
                        .AsSingleton<PlaylistsPageView>();
            registration.Register<PlaylistsPageViewPresenter>().AsSingleton();
            registration.Register<PlaylistsPageViewBindingModel>();

            // User Playlists view
            registration.Register<IUserPlaylistsPageView>()
                        .InjectionRule<BindingModelBase, UserPlaylistsPageViewPresenter>()
                        .AsSingleton<PlaylistsPageView>();
            registration.Register<UserPlaylistsPageViewPresenter>().AsSingleton();

            // Playlist view (playlist, all artist's songs, system playlists, genre)
            registration.Register<IPlaylistPageView>()
                        .InjectionRule<BindingModelBase, PlaylistPageViewPresenter>()
                        .AsSingleton<PlaylistPageView>();
            registration.Register<PlaylistPageViewPresenter>().AsSingleton();
            registration.Register<PlaylistPageViewBindingModel>().AsSingleton();

            // Queue view
            registration.Register<ICurrentPlaylistPageView>()
                        .InjectionRule<BindingModelBase, CurrentPlaylistPageViewPresenter>()
                        .AsSingleton<CurrentPlaylistPageView>();
            registration.Register<CurrentPlaylistPageViewPresenter>().AsSingleton();

            // Search page
            registration.Register<ISearchPageView>()
                        .InjectionRule<BindingModelBase, SearchPageViewPresenter>()
                        .AsSingleton<SearchPageView>();
            registration.Register<SearchPageViewPresenter>().AsSingleton();
            registration.Register<SearchPageViewBindingModel>().AsSingleton();

            // Selected artist view (with list of albums)
            registration.Register<IArtistPageView>()
                        .InjectionRule<BindingModelBase, ArtistPageViewPresenter>()
                        .AsSingleton<ArtistPageView>();
            registration.Register<ArtistPageViewPresenter>().AsSingleton();
            registration.Register<ArtistPageViewBindingModel>().AsSingleton();

            // Album page view
            registration.Register<IAlbumPageView>()
                        .InjectionRule<BindingModelBase, AlbumPageViewPresenter>()
                        .AsSingleton<AlbumPageView>();
            registration.Register<AlbumPageViewPresenter>().AsSingleton();

            // Radio stations view
            registration.Register<IRadioPageView>()
                        .InjectionRule<BindingModelBase, RadioPageViewPresenter>()
                        .AsSingleton<PlaylistsPageView>();
            registration.Register<RadioPageViewPresenter>().AsSingleton();

            // Genres view
            registration.Register<IGenrePageView>()
                        .InjectionRule<BindingModelBase, GenrePageViewPresenter>()
                        .AsSingleton<PlaylistsPageView>();
            registration.Register<GenrePageViewPresenter>().AsSingleton();

            // Explore view
            registration.Register<IExplorePageView>()
                        .InjectionRule<BindingModelBase, ExplorePageViewPresenter>()
                        .AsSingleton<ExplorePageView>();
            registration.Register<ExplorePageViewPresenter>().AsSingleton();

            // Situations view
            registration.Register<ISituationsPageView>()
                       .InjectionRule<BindingModelBase, SituationsPageViewPresenter>()
                       .AsSingleton<SituationsPageView>();
            registration.Register<SituationsPageViewPresenter>().AsSingleton();
            registration.Register<SituationsPageViewBindingModel>();

            // Situations stations view
            registration.Register<ISituationStationsPageView>()
                       .InjectionRule<BindingModelBase, SituationStationsPageViewPresenter>()
                       .AsSingleton<SituationStationsPageView>();
            registration.Register<SituationStationsPageViewPresenter>().AsSingleton();
            registration.Register<SituationStationsPageViewBindingModel>();
        }

        public static void RegisterSettingViews(IRegistrationContext registration)
        {
            registration.Register<AccountsView>()
                            .InjectionRule<BindingModelBase, AccountsViewPresenter>();
            registration.Register<AccountsViewPresenter>();

            registration.Register<AppSettingsView>()
                            .InjectionRule<BindingModelBase, AppSettingsViewPresenter>();
            registration.Register<AppSettingsViewPresenter>();

            registration.Register<OfflineCacheView>()
                            .InjectionRule<BindingModelBase, OfflineCacheViewPresenter>();
            registration.Register<OfflineCacheViewPresenter>();
            registration.Register<OfflineCacheViewBindingModel>();

            registration.Register<SupportView>()
                .InjectionRule<BindingModelBase, SupportViewPresenter>();
            registration.Register<SupportViewPresenter>();

            registration.Register<PrivacyView>();
            registration.Register<LegalView>();
        }

        public static void RegisterPopupViews(IRegistrationContext registration)
        {
            // Authentification popup
            registration.Register<IAuthentificationPopupView>()
                        .InjectionRule<BindingModelBase, AuthentificationPopupViewPresenter>()
                        .As<AuthentificationPopupView>();
            registration.Register<AuthentificationPopupViewPresenter>();

            // Progress loading view
            registration.Register<IProgressLoadingPopupView>()
                        .InjectionRule<BindingModelBase, ProgressLoadingPopupViewPresenter>()
                        .As<ProgressLoadingPopupView>();
            registration.Register<ProgressLoadingPopupViewPresenter>();

            // The releases history
            registration.Register<IReleasesHistoryPopupView>()
                        .InjectionRule<BindingModelBase, ReleasesHistoryPopupViewPresenter>()
                        .As<ReleasesHistoryPopupView>();
            registration.Register<ReleasesHistoryPopupViewPresenter>();

            registration.Register<IAddToPlaylistPopupView>()
                        .InjectionRule<BindingModelBase, AddToPlaylistPopupViewPresenter>()
                        .As<AddToPlaylistPopupView>();
            registration.Register<AddToPlaylistPopupViewPresenter>();

            registration.Register<IPlaylistEditPopupView>()
                        .InjectionRule<BindingModelBase, PlaylistEditPopupViewPresenter>()
                        .As<PlaylistEditPopupView>();
            registration.Register<PlaylistEditPopupViewPresenter>();

            registration.Register<IQueueActionsPopupView>()
                        .InjectionRule<BindingModelBase, QueueActionsPopupViewPresenter>()
                        .As<QueueActionsPopupView>();
            registration.Register<QueueActionsPopupViewPresenter>();

            registration.Register<IRadioEditPopupView>()
                        .InjectionRule<BindingModelBase, RadioEditPopupViewPresenter>()
                        .As<PlaylistEditPopupView>();
            registration.Register<RadioEditPopupViewPresenter>();

            registration.Register<IReadMorePopup>()
                        .InjectionRule<BindingModelBase, ReadMorePopupPresenter>()
                        .As<ReadMorePopup>();
            registration.Register<ReadMorePopupPresenter>();

            registration.Register<IDonatePopupView>()
                       .InjectionRule<BindingModelBase, DonatePopupViewPresenter>()
                       .As<DonatePopupView>();
            registration.Register<DonatePopupViewPresenter>();

            registration.Register<ITutorialPopupView>()
                       .InjectionRule<BindingModelBase, TutorialPopupViewPresenter>()
                       .As<TutorialPopupView>();
            registration.Register<TutorialPopupViewPresenter>();

            registration.Register<ICacheMovePopupView>()
                       .InjectionRule<BindingModelBase, CacheMovePopupViewPresenter>()
                       .As<CacheMovePopupView>();
            registration.Register<CacheMovePopupViewPresenter>();
        }

        public static void RegisterViews(IRegistrationContext registration)
        {
            registration.Register<IPlayerView>()
                        .InjectionRule<BindingModelBase, PlayerViewPresenter>()
                        .AsSingleton<PlayerView>();

            registration.Register<PlayerBindingModel>().AsSingleton();
            registration.Register<PlayerViewPresenter>().AsSingleton();

            registration.Register<LinksRegionView>()
                        .InjectionRule<BindingModelBase, LinksRegionViewPresenter>();
            registration.Register<LinksRegionViewPresenter>();

            registration.Register<IMainMenu>()
                       .InjectionRule<BindingModelBase, MainMenuPresenter>()
                       .AsSingleton<MainMenu>();
            registration.Register<MainMenuPresenter>();

            registration.Register<ISongsListView>()
                       .InjectionRule<BindingModelBase, SongsListViewPresenter>()
                       .As<SongsListView>();
            registration.Register<SongsListViewPresenter>();

            registration.Register<IPlaylistsListView>()
                       .InjectionRule<BindingModelBase, PlaylistsListViewPresenter>()
                       .As<PlaylistsListView>();
            registration.Register<PlaylistsListViewPresenter>();
        }
    }
}
