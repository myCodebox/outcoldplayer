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

            if (playlist is Album)
            {
                return @this.NavigateTo<IAlbumPageView>(
                    new PlaylistNavigationRequest()
                        {
                            PlaylistId = ((Album)playlist).AlbumId,
                            PlaylistType = PlaylistType.Album
                        });
            }
            else if (playlist is Artist)
            {
                return @this.NavigateTo<IArtistPageView>(
                    new PlaylistNavigationRequest()
                    {
                        PlaylistId = ((Artist)playlist).ArtistId,
                        PlaylistType = PlaylistType.Artist
                    });
            }
            else
            {
                var request = new PlaylistNavigationRequest();

                if (playlist is Genre)
                {
                    request.PlaylistType = PlaylistType.Genre;
                    request.PlaylistId = ((Genre)playlist).GenreId;
                }
                else if (playlist is UserPlaylist)
                {
                    request.PlaylistType = PlaylistType.UserPlaylist;
                    request.PlaylistId = ((UserPlaylist)playlist).PlaylistId;
                }
                else if (playlist is SystemPlaylist)
                {
                    request.PlaylistType = PlaylistType.SystemPlaylist;
                    request.PlaylistId = (int)((SystemPlaylist)playlist).SystemPlaylistType;
                }
                else
                {
                    throw new ArgumentException("Unknown songs container", "playlist");
                }

                return @this.NavigateTo<IPlaylistPageView>(request);
            }
        }
    }
}
