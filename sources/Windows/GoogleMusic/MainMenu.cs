// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;

    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Views;

    public static class MainMenu
    {
        public static void Initialize(
            IMainFrame applicationToolbar, 
            IApplicationResources resources, 
            IApplicationStateService stateService,
            IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<ApplicationStateChangeEvent>().Subscribe(
                (e) => applicationToolbar.SetMenuItems(GetItems(resources, stateService)));

            applicationToolbar.SetMenuItems(GetItems(resources, stateService)); 
        }

        public static IEnumerable<MenuItemMetadata> GetItems(IApplicationResources resources, IApplicationStateService stateService)
        {
            yield return MenuItemMetadata.FromViewType<IStartPageView>(new { Title = resources.GetString("MainMenu_Home"), Icon = "ms-appx:///Resources/home.png" });
            yield return MenuItemMetadata.FromViewType<ICurrentPlaylistPageView>(new { Title = resources.GetString("MainMenu_Queue"), Icon = "ms-appx:///Resources/queue.png" });
            yield return MenuItemMetadata.FromViewType<IUserPlaylistsPageView>(new { Title = resources.GetString("MainMenu_Playlists"), Icon = "ms-appx:///Resources/playlists.png" }, PlaylistType.UserPlaylist);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>(new { Title = resources.GetString("MainMenu_Artists"), Icon = "ms-appx:///Resources/artists.png" }, PlaylistType.Artist);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>(new { Title = resources.GetString("MainMenu_Albums"), Icon = "ms-appx:///Resources/albums.png" }, PlaylistType.Album);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>(new { Title = resources.GetString("MainMenu_Genres"), Icon = "ms-appx:///Resources/genres.png" }, PlaylistType.Genre);
            if (stateService.CurrentState == ApplicationState.Online)
            {
                yield return MenuItemMetadata.FromViewType<IRadioPageView>(new { Title = resources.GetString("MainMenu_Radio"), Icon = "ms-appx:///Resources/Radio.png" }, PlaylistType.Radio);
            }
        }
    }
}