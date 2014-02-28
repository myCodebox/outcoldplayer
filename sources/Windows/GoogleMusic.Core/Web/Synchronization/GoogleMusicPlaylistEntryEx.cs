// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Synchronization
{
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public static class GoogleMusicPlaylistEntryEx
    {
        public static UserPlaylistEntry ToUserPlaylistEntry(this GoogleMusicPlaylistEntry googleMusicPlaylistEntry)
        {
            var entry = new UserPlaylistEntry();

            Mapper(googleMusicPlaylistEntry, entry);

            return entry;
        }

        public static void Mapper(GoogleMusicPlaylistEntry googleMusicPlaylistEntry, UserPlaylistEntry entry)
        {
            entry.Id = googleMusicPlaylistEntry.Id;
            entry.PlaylistOrder = googleMusicPlaylistEntry.AbsolutePosition;
            entry.SongId = googleMusicPlaylistEntry.TrackId;
            entry.PlaylistId = googleMusicPlaylistEntry.PlaylistId;
            entry.CreationDate = DateTimeExtensions.FromUnixFileTime(googleMusicPlaylistEntry.CreationTimestamp / 1000);
            entry.LastModified =
                DateTimeExtensions.FromUnixFileTime(googleMusicPlaylistEntry.LastModifiedTimestamp / 1000);
            entry.CliendId = googleMusicPlaylistEntry.ClientId;
            entry.Source = googleMusicPlaylistEntry.Source;
        }
    }
}
