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
            yield return MenuItemMetadata.FromViewType<IStartPageView>(new { Title = "Home", Icon = "ms-appx:///Resources/home.png" });
            yield return MenuItemMetadata.FromViewType<ICurrentPlaylistPageView>(new { Title = "Queue", Icon = "ms-appx:///Resources/queue.png" });
            yield return MenuItemMetadata.FromViewType<IUserPlaylistsPageView>(new { Title = "Playlists", Icon = "ms-appx:///Resources/playlists.png" }, PlaylistType.UserPlaylist);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>(new { Title = "Artists", Icon = "ms-appx:///Resources/artists.png" }, PlaylistType.Artist);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>(new { Title = "Albums", Icon = "ms-appx:///Resources/albums.png" }, PlaylistType.Album);
            yield return MenuItemMetadata.FromViewType<IPlaylistsPageView>(new { Title = "Genres", Icon = "ms-appx:///Resources/genres.png" }, PlaylistType.Genre);
        }
    }
}