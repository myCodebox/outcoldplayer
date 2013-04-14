// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    internal static class PlaylistTypeEx
    {
        public static string ToTitle(this PlaylistType playlistType)
        {
            switch (playlistType)
            {
                case PlaylistType.Album:
                    return "Album";
                case PlaylistType.Artist:
                    return "Artist";
                case PlaylistType.Genre:
                    return "Genre";
                case PlaylistType.UserPlaylist:
                    return "Playlist";
                default:
                    return null;
            }
        }

        public static string ToPluralTitle(this PlaylistType playlistType)
        {
            switch (playlistType)
            {
                case PlaylistType.Album:
                    return "Albums";
                case PlaylistType.Artist:
                    return "Artists";
                case PlaylistType.Genre:
                    return "Genres";
                case PlaylistType.UserPlaylist:
                    return "Playlists";
                default:
                    return null;
            }
        }
    }
}
