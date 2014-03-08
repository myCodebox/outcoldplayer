// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Windows.UI.Popups;

    using OutcoldSolutions.GoogleMusic.Models;

    public class DownloadAction : ISelectedObjectAction
    {
        private readonly IApplicationResources applicationResources;
        private readonly IApplicationStateService stateService;
        private readonly IPlaylistsService playlistsService;
        private readonly ISongsCachingService songsCachingService;
        private readonly ISongsService songsService;

        public DownloadAction(
            IApplicationResources applicationResources,
            IApplicationStateService stateService,
            IPlaylistsService playlistsService,
            ISongsCachingService songsCachingService,
            ISongsService songsService)
        {
            this.applicationResources = applicationResources;
            this.stateService = stateService;
            this.playlistsService = playlistsService;
            this.songsCachingService = songsCachingService;
            this.songsService = songsService;
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
                return this.applicationResources.GetString("Toolbar_KeepLocal");
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
                        || (playlist.PlaylistType == PlaylistType.UserPlaylist && ((UserPlaylist)playlist).IsShared))
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
                    songs.AddRange(await this.playlistsService.GetSongsAsync((IPlaylist)obj));
                }
            }

            if (selectedObjects.Count > 0)
            {
                var yesUiCommand = new UICommand("Yes");
                var ignoreUiCommand = new UICommand("Ignore");
                var cancelUiCommand = new UICommand("Cancel");

                MessageDialog dialog = new MessageDialog("Some of the selected songs are not in your library or playlists. "
                                                         + "To download them to your cache you need first add them to your library or one of playlists. "
                                                         + "Do you want to add them to your library now?");
                dialog.Commands.Add(yesUiCommand);
                dialog.Commands.Add(ignoreUiCommand);
                dialog.Commands.Add(cancelUiCommand);

                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 2;

                var command = await dialog.ShowAsync();

                if (command == cancelUiCommand)
                {
                    return null;
                }

                if (command == yesUiCommand)
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
