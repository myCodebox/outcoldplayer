// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    using OutcoldSolutions.GoogleMusic.Services;

    public static class PlaylistTypeEx
    {
        private static readonly Lazy<ISettingsService> SettingsService = new Lazy<ISettingsService>(() => ApplicationContext.Container.Resolve<ISettingsService>()); 

        public static string GetTitle(PlaylistType playlistType)
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
                case PlaylistType.Radio:
                    return SettingsService.Value.GetIsAllAccessAvailable() ? "Radio" : "Instant Mix";
                default:
                    return null;
            }
        }

        public static string GetPluralTitle(PlaylistType playlistType)
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
                case PlaylistType.Radio:
                    return SettingsService.Value.GetIsAllAccessAvailable() ? "Radio Stations" : "Instant Mixes";
                default:
                    return null;
            }
        }
    }
}
