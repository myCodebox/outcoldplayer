// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Views;

    public static class NavigationServiceEx 
    {
        public static IPageView NavigateToPlaylist(this INavigationService @this, IPlaylist playlist)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            if (playlist == null)
            {
                throw new ArgumentNullException("playlist");
            }

            var request = new PlaylistNavigationRequest(playlist);

            return @this.NavigateToPlaylist(request);
        }

        public static IPageView NavigateToPlaylist(this INavigationService @this, PlaylistNavigationRequest request)
        {
            if (request.PlaylistType == PlaylistType.Album)
            {
                return @this.NavigateTo<IAlbumPageView>(request);
            }
            else if (request.PlaylistType == PlaylistType.Artist)
            {
                return @this.NavigateTo<IArtistPageView>(request);
            }
            else if (request.PlaylistType == PlaylistType.Genre)
            {
                return @this.NavigateTo<IGenrePageView>(request);
            }
            else
            {
                return @this.NavigateTo<IPlaylistPageView>(request);
            }
        }
    }
}
