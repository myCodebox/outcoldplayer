// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    using OutcoldSolutions.GoogleMusic.Services;

    internal static class PlaylistTypeEx
    {
        private static readonly Lazy<ISettingsService> SettingsService = new Lazy<ISettingsService>(() => ApplicationBase.Container.Resolve<ISettingsService>()); 

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
                case PlaylistType.Radio:
                    return SettingsService.Value.GetIsAllAccessAvailable() ? resources.GetString("Model_Radio_Title") : resources.GetString("Model_InstantMix_Title");
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
                case PlaylistType.Radio:
                    return SettingsService.Value.GetIsAllAccessAvailable() ? resources.GetString("Model_Radio_Plural_Title") : resources.GetString("Model_InstantMixes_Plural_Title");
                default:
                    return null;
            }
        }
    }
}
