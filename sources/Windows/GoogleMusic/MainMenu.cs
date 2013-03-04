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
        public static void Initialize(IApplicationToolbar applicationToolbar)
        {
            applicationToolbar.SetMenuItems(GetItems());
        }

        public static IEnumerable<MenuItemMetadata> GetItems()
        {
            yield return MenuItemMetadata.FromViewType<IStartPageView>("Home");
            yield return MenuItemMetadata.FromViewType<ICurrentPlaylistPageView>("Queue");
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>("Playlists", PlaylistsRequest.Playlists);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>("Artists", PlaylistsRequest.Artists);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>("Albums", PlaylistsRequest.Albums);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>("Genres", PlaylistsRequest.Genres);
        }
    }
}