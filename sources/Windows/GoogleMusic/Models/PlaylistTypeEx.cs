// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    internal static class PlaylistTypeEx
    {
        public static string GetTitle(this IApplicationResources resources, PlaylistType playlistType)
        {
            switch (playlistType)
            {
                case PlaylistType.Album:
                    return resources.GetString("Model_Album_Title");
                case PlaylistType.Artist:
                    return resources.GetString("Model_Artist_Title");
                case PlaylistType.Genre:
                    return resources.GetString("Model_Genre_Title");
                case PlaylistType.UserPlaylist:
                    return resources.GetString("Model_UserPlaylist_Title");
                default:
                    return null;
            }
        }

        public static string GetPluralTitle(this IApplicationResources resources, PlaylistType playlistType)
        {
            switch (playlistType)
            {
                case PlaylistType.Album:
                    return resources.GetString("Model_Album_Plural_Title");
                case PlaylistType.Artist:
                    return resources.GetString("Model_Artist_Plural_Title");
                case PlaylistType.Genre:
                    return resources.GetString("Model_Genre_Plural_Title");
                case PlaylistType.UserPlaylist:
                    return resources.GetString("Model_UserPlaylist_Plural_Title");
                default:
                    return null;
            }
        }
    }
}
