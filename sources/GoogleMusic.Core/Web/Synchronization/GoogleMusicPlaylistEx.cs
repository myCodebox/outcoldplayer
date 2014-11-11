// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Synchronization
{
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public static class GoogleMusicPlaylistEx
    {
        public static UserPlaylist ToUserPlaylist(this GoogleMusicPlaylist googleMusicPlaylist)
        {
            var userPlaylist = new UserPlaylist();

            Mapper(googleMusicPlaylist, userPlaylist);

            return userPlaylist;
        }

        public static void Mapper(GoogleMusicPlaylist googleMusicPlaylist, UserPlaylist userPlaylist)
        {
            userPlaylist.PlaylistId = googleMusicPlaylist.Id;
            userPlaylist.Title = googleMusicPlaylist.Name;
            userPlaylist.TitleNorm = googleMusicPlaylist.Name.Normalize();
            userPlaylist.CreationDate =
                DateTimeExtensions.FromUnixFileTime(googleMusicPlaylist.CreationTimestramp / 1000);
            userPlaylist.LastModified =
                DateTimeExtensions.FromUnixFileTime(googleMusicPlaylist.LastModifiedTimestamp / 1000);
            userPlaylist.RecentDate = DateTimeExtensions.FromUnixFileTime(googleMusicPlaylist.RecentTimestamp / 1000);
            userPlaylist.Type = googleMusicPlaylist.Type;
            userPlaylist.ShareToken = googleMusicPlaylist.ShareToken;
            userPlaylist.OwnerName = googleMusicPlaylist.OwnerName;
            userPlaylist.OwnerProfilePhotoUrl = googleMusicPlaylist.OwnerProfilePhotoUrl;
            userPlaylist.AccessControlled = googleMusicPlaylist.AccessControlled;
        }
    }
}
