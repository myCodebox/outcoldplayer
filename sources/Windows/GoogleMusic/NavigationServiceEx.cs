// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Views;

    public static class NavigationServiceEx 
    {
        public static IPageView NavigateToPlaylist(this INavigationService @this, ISongsContainer songsContainer)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            if (songsContainer == null)
            {
                throw new ArgumentNullException("songsContainer");
            }

            if (songsContainer is Album)
            {
                return @this.NavigateTo<IAlbumPageView>(
                    new PlaylistNavigationRequest()
                        {
                            PlaylistId = ((Album)songsContainer).AlbumId,
                            SongsContainerType = SongsContainerType.Album
                        });
            }
            else if (songsContainer is Artist)
            {
                return @this.NavigateTo<IArtistPageView>(
                    new PlaylistNavigationRequest()
                    {
                        PlaylistId = ((Artist)songsContainer).ArtistId,
                        SongsContainerType = SongsContainerType.Artist
                    });
            }
            else
            {
                var request = new PlaylistNavigationRequest();

                if (songsContainer is Genre)
                {
                    request.SongsContainerType = SongsContainerType.Genre;
                    request.PlaylistId = ((Genre)songsContainer).GenreId;
                }
                else if (songsContainer is UserPlaylist)
                {
                    request.SongsContainerType = SongsContainerType.UserPlaylist;
                    request.PlaylistId = ((UserPlaylist)songsContainer).PlaylistId;
                }
                else if (songsContainer is SystemPlaylist)
                {
                    request.SongsContainerType = SongsContainerType.SystemPlaylist;
                    request.PlaylistId = (int)((SystemPlaylist)songsContainer).SystemPlaylistType;
                }
                else
                {
                    throw new ArgumentException("Unknown songs container", "songsContainer");
                }

                return @this.NavigateTo<IPlaylistPageView>(request);
            }
        }
    }
}
