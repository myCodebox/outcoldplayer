//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.BindingModels;
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
            // Initialization page
            registration.Register<IInitPageView>()
                            .InjectionRule<BindingModelBase, InitPageViewPresenter>()
                            .As<InitPageView>();
            registration.Register<InitPageViewPresenter>();

            // Authentification page
            registration.Register<IAuthentificationPageView>()
                        .InjectionRule<BindingModelBase, AuthentificationPageViewPresenter>()
                        .As<AuthentificationPageView>();
            registration.Register<AuthentificationPageViewPresenter>();

            // Progress loading view
            registration.Register<IProgressLoadingView>()
                        .InjectionRule<BindingModelBase, ProgressLoadingPageViewPresenter>()
                        .As<ProgressLoadingPageView>();
            registration.Register<ProgressLoadingPageViewPresenter>();

            // Start page view
            registration.Register<IStartPageView>()
                        .InjectionRule<BindingModelBase, StartPageViewPresenter>()
                        .AsSingleton<StartPageView>();
            registration.Register<StartPageViewPresenter>().AsSingleton();
            registration.Register<StartViewBindingModel>().AsSingleton();

            // Playlists view (albums, playlists, genres, artists)
            registration.Register<IPlaylistsPageView>()
                        .InjectionRule<BindingModelBase, PlaylistsPageViewPresenter>()
                        .AsSingleton<PlaylistsPageView>();
            registration.Register<PlaylistsPageViewPresenter>().AsSingleton();
            registration.Register<PlaylistsPageViewBindingModel>().AsSingleton();

            // Playlist view (playlist, all artist's songs, system playlists, genre)
            registration.Register<IPlaylistPageView>()
                        .InjectionRule<BindingModelBase, PlaylistPageViewPresenter>()
                        .AsSingleton<PlaylistPageView>();
            registration.Register<PlaylistPageViewPresenter>().AsSingleton();
            registration.Register<PlaylistPageViewBindingModel<PlaylistBaseBindingModel>>().AsSingleton();

            // Queue view
            registration.Register<ICurrentPlaylistPageView>()
                        .InjectionRule<BindingModelBase, CurrentPlaylistPageViewPresenter>()
                        .AsSingleton<CurrentPlaylistPageView>();
            registration.Register<CurrentPlaylistPageViewPresenter>().AsSingleton();
            registration.Register<CurrentPlaylistPageViewBindingModel>().AsSingleton();

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
            registration.Register<PlaylistPageViewBindingModel<AlbumBindingModel>>().AsSingleton();

            // The releases history
            registration.Register<IReleasesHistoryPageView>()
                        .InjectionRule<BindingModelBase, ReleasesHistoryPageViewPresenter>()
                        .As<ReleasesHistoryPageView>();
            registration.Register<ReleasesHistoryPageViewPresenter>();
        }

        public static void RegisterSettingViews(IRegistrationContext registration)
        {
            registration.Register<AccountsView>()
                            .InjectionRule<BindingModelBase, AccountsViewPresenter>();
            registration.Register<AccountsViewPresenter>();

            registration.Register<UpgradeView>()
                        .InjectionRule<BindingModelBase, UpgradeViewPresenter>();
            registration.Register<UpgradeViewPresenter>();

            registration.Register<PrivacyView>();
            registration.Register<SupportView>();
        }

        public static void RegisterViewResolvers(IRegistrationContext registration)
        {
            registration.Register<PlaylistViewResolver>();
        }

        public static void RegisterPopupViews(IRegistrationContext registration)
        {
            registration.Register<IAddToPlaylistPopupView>()
                            .InjectionRule<BindingModelBase, AddToPlaylistPopupViewPresenter>()
                            .As<AddToPlaylistPopupView>();
            registration.Register<AddToPlaylistPopupViewPresenter>();
        }

    }
}
