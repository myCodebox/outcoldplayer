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
        public static void Initialize(IMainFrame applicationToolbar)
        {
            applicationToolbar.SetMenuItems(GetItems());
        }

        public static IEnumerable<MenuItemMetadata> GetItems()
        {
            yield return MenuItemMetadata.FromViewType<IStartPageView>("Home");
            yield return MenuItemMetadata.FromViewType<ICurrentPlaylistPageView>("Queue");
            yield return MenuItemMetadata.FromViewType<IUserPlaylistsPageView>("Playlists", PlaylistType.UserPlaylist);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>("Artists", PlaylistType.Artist);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>("Albums", PlaylistType.Album);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>("Genres", PlaylistType.Genre);
        }
    }
}