// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Views;

    public static class MainMenu
    {
        public static void Initialize(
            IMainFrame applicationToolbar, IApplicationResources resources)
        {
            applicationToolbar.SetMenuItems(GetItems(resources));
        }

        public static IEnumerable<MenuItemMetadata> GetItems(IApplicationResources resources)
        {
            yield return MenuItemMetadata.FromViewType<IStartPageView>(new { Title = resources.GetString("MainMenu_Home"), Icon = "ms-appx:///Resources/home.png" });
            yield return MenuItemMetadata.FromViewType<ICurrentPlaylistPageView>(new { Title = resources.GetString("MainMenu_Queue"), Icon = "ms-appx:///Resources/queue.png" });
            yield return MenuItemMetadata.FromViewType<IUserPlaylistsPageView>(new { Title = resources.GetString("MainMenu_Playlists"), Icon = "ms-appx:///Resources/playlists.png" }, PlaylistType.UserPlaylist);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>(new { Title = resources.GetString("MainMenu_Artists"), Icon = "ms-appx:///Resources/artists.png" }, PlaylistType.Artist);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>(new { Title = resources.GetString("MainMenu_Albums"), Icon = "ms-appx:///Resources/albums.png" }, PlaylistType.Album);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>(new { Title = resources.GetString("MainMenu_Genres"), Icon = "ms-appx:///Resources/genres.png" }, PlaylistType.Genre);
        }
    }
}