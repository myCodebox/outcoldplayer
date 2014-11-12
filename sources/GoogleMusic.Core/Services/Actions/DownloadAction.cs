// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public class DownloadAction : ISelectedObjectAction
    {
        private readonly IApplicationStateService stateService;
        private readonly IPlaylistsService playlistsService;
        private readonly ISongsCachingService songsCachingService;
        private readonly ISongsService songsService;
        private readonly INotificationService notificationService;

        public DownloadAction(
            IApplicationStateService stateService,
            IPlaylistsService playlistsService,
            ISongsCachingService songsCachingService,
            ISongsService songsService,
            INotificationService notificationService)
        {
            this.stateService = stateService;
            this.playlistsService = playlistsService;
            this.songsCachingService = songsCachingService;
            this.songsService = songsService;
            this.notificationService = notificationService;
        }

        public string Icon
        {
            get
            {
                return CommandIcon.Pin;
            }
        }

        public string Title
        {
            get
            {
                return "Keep on device";
            }
        }

        public ActionGroup Group
        {
            get
            {
                return ActionGroup.Cache;
            }
        }

        public int Priority
        {
            get
            {
                return 1000;
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            if (!this.stateService.IsOnline())
            {
                return false;
            }

            if (selectedObjects.Count == 0)
            {
                return false;
            }

            bool isSongsCollection = false;
            bool hasNotCached = false;
            foreach (var obj in selectedObjects)
            {
                var song = obj as Song;
                if (song != null)
                {
                    isSongsCollection = true;
                    if (!song.IsCached)
                    {
                        hasNotCached = true;
                    }
                }
                else
                {
                    if (isSongsCollection)
                    {
                        return false;
                    }

                    var playlist = (IPlaylist)obj;
                    if (playlist.PlaylistType == PlaylistType.Radio
                        || playlist.PlaylistType == PlaylistType.AllAccessGenre
                        || playlist.PlaylistType == PlaylistType.Situation
                        || playlist.PlaylistType == PlaylistType.SituationStations
                        || playlist.PlaylistType == PlaylistType.SituationRadio)
                    {
                        return false;
                    }

                    hasNotCached |= ((playlist.SongsCount - playlist.OfflineSongsCount) > 0);
                }
            }

            return hasNotCached;
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            List<Song> songs = new List<Song>();
            List<Song> unknownSongs = new List<Song>();

            foreach (var obj in selectedObjects)
            {
                var song = obj as Song;
                if (song != null)
                {
                    if (song.UnknownSong)
                    {
                        unknownSongs.Add(song);
                    }
                    else
                    {
                        songs.Add(song);
                    }
                }
                else
                {
                    foreach (var plSong in await this.playlistsService.GetSongsAsync((IPlaylist)obj))
                    {
                        if (plSong.UnknownSong)
                        {
                            unknownSongs.Add(plSong);
                        }
                        else
                        {
                            songs.Add(plSong);
                        }
                    }
                }
            }

            if (unknownSongs.Count > 0)
            {
                bool? result = await this.notificationService.ShowQuestionAsync(
                    "Some of the selected songs are not in your library or playlists. "
                    + "To download them to your cache you need first add them to your library or one of your playlists. "
                    + "Do you want to add them to your library now?",
                    yesButton: "Yes",
                    noButton: "Ignore",
                    cancelButton: "Cancel");

                if (!result.HasValue)
                {
                    return null;
                }
                
                if (result.Value)
                {
                    IList<Song> addedSongs = await this.songsService.AddToLibraryAsync(unknownSongs);
                    if (addedSongs == null)
                    {
                        return false;
                    }
                    else
                    {
                        songs.AddRange(addedSongs);
                    }
                }
            }

            await this.songsCachingService.QueueForDownloadAsync(songs);

            return true;
        }
    }
}
