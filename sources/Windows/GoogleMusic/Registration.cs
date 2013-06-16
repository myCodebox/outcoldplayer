//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.BindingModels.Popups;
    using OutcoldSolutions.GoogleMusic.BindingModels.Settings;
    using OutcoldSolutions.GoogleMusic.Models;
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
            registration.Register<IStartPageView>()
                        .InjectionRule<BindingModelBase, StartPageViewPresenter>()
                        .AsSingleton<StartPageView>();
            registration.Register<StartPageViewPresenter>().AsSingleton();
            registration.Register<StartViewBindingModel>().AsSingleton();

            // Playlists view (albums, genres, artists)
            registration.Register<IPlaylistsPageView>()
                        .InjectionRule<BindingModelBase, PlaylistsPageViewPresenter>()
                        .AsSingleton<PlaylistsPageView>();
            registration.Register<PlaylistsPageViewPresenter>().AsSingleton();
            registration.Register<PlaylistsPageViewBindingModel>();

            // User Playlists view
            registration.Register<IUserPlaylistsPageView>()
                        .InjectionRule<BindingModelBase, UserPlaylistsPageViewPresenter>()
                        .AsSingleton<UserPlaylistsPageView>();
            registration.Register<UserPlaylistsPageViewPresenter>().AsSingleton();

            // Playlist view (playlist, all artist's songs, system playlists, genre)
            registration.Register<IPlaylistPageView>()
                        .InjectionRule<BindingModelBase, PlaylistPageViewPresenter>()
                        .AsSingleton<PlaylistPageView>();
            registration.Register<PlaylistPageViewPresenter>().AsSingleton();
            registration.Register<PlaylistPageViewBindingModel<IPlaylist>>().AsSingleton();

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
            registration.Register<PlaylistPageViewBindingModel<Album>>().AsSingleton();

            // Radio page view
            registration.Register<IRadioStationsView>()
                        .InjectionRule<BindingModelBase, RadioStationsViewPresenter>()
                        .AsSingleton<RadioStationsView>();
            registration.Register<RadioStationsViewPresenter>().AsSingleton();
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

            registration.Register<UpgradeView>()
                        .InjectionRule<BindingModelBase, UpgradeViewPresenter>();
            registration.Register<UpgradeViewPresenter>();

            registration.Register<PrivacyView>();
            registration.Register<SupportView>();
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

            registration.Register<IPlayerMorePopupView>()
                        .InjectionRule<BindingModelBase, PlayerMorePopupViewPresenter>()
                        .As<PlayerMorePopupView>();
            registration.Register<PlayerMorePopupViewBindingModel>();
            registration.Register<PlayerMorePopupViewPresenter>();

            registration.Register<IPlaylistEditPopupView>()
                        .InjectionRule<BindingModelBase, PlaylistEditPopupViewPresenter>()
                        .As<PlaylistEditPopupView>();
            registration.Register<PlaylistEditPopupViewPresenter>();

            registration.Register<IQueueActionsPopupView>()
                        .InjectionRule<BindingModelBase, QueueActionsPopupViewPresenter>()
                        .As<QueueActionsPopupView>();
            registration.Register<QueueActionsPopupViewPresenter>();
        }

        public static void RegisterViews(IRegistrationContext registration)
        {
            registration.Register<IPlayerView>()
                        .InjectionRule<BindingModelBase, PlayerViewPresenter>()
                        .AsSingleton<PlayerView>();

            registration.Register<PlayerBindingModel>().AsSingleton();
            registration.Register<PlayerViewPresenter>().AsSingleton();

            registration.Register<ISnappedPlayerView>()
                        .InjectionRule<BindingModelBase, SnappedPlayerViewPresenter>()
                        .AsSingleton<SnappedPlayerView>();

            registration.Register<SnappedPlayerBindingModel>().AsSingleton();
            registration.Register<SnappedPlayerViewPresenter>().AsSingleton();

            registration.Register<LinksRegionView>()
                        .InjectionRule<BindingModelBase, LinksRegionViewPresenter>();
            registration.Register<LinksRegionViewPresenter>();
        }
    }
}
