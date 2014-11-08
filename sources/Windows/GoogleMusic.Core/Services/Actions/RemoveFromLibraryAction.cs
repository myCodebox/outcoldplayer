// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using Windows.UI.Popups;

    using OutcoldSolutions.GoogleMusic.Models;

    public class RemoveFromLibraryAction: ISelectedObjectAction
    {
        private readonly IApplicationResources applicationResources;

        private readonly IApplicationStateService stateService;

        private readonly IPlaylistsService playlistsService;

        private readonly ISongsCachingService songsCachingService;

        private readonly ISongsService songsService;

        public RemoveFromLibraryAction(
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
                return CommandIcon.Remove;
            }
        }

        public string Title
        {
            get
            {
                return "Remove from library";
            }
        }

        public ActionGroup Group
        {
            get
            {
                return ActionGroup.Library;
            }
        }

        public int Priority
        {
            get
            {
                return 100;
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            if (!this.stateService.IsOnline())
            {
                return false;
            }

            bool hasLibrary = false;
            foreach (var obj in selectedObjects)
            {
                var song = obj as Song;
                if (song != null)
                {
                    if (song.IsLibrary)
                    {
                        hasLibrary = true;
                    }
                }
                else
                {
                    var playlist = (IPlaylist)obj;
                    if (playlist.PlaylistType == PlaylistType.Radio 
                        || playlist.PlaylistType == PlaylistType.UserPlaylist
                        || playlist.PlaylistType == PlaylistType.SystemPlaylist
                        || playlist.PlaylistType == PlaylistType.AllAccessGenre
                        || playlist.PlaylistType == PlaylistType.Situation
                        || playlist.PlaylistType == PlaylistType.SituationStations
                        || playlist.PlaylistType == PlaylistType.SituationRadio)
                    {
                        return false;
                    }

                    if (playlist.PlaylistType == PlaylistType.Artist && string.IsNullOrEmpty(playlist.Id))
                    {
                        return false;
                    }

                    hasLibrary |= !string.IsNullOrEmpty(playlist.Id);
                }
            }

            return hasLibrary;
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            List<Song> songs = new List<Song>();

            foreach (var obj in selectedObjects)
            {
                var song = obj as Song;
                if (song != null)
                {
                    if (song.IsLibrary)
                    {
                        songs.Add(song);
                    }
                }
                else
                {
                    var playlist = (IPlaylist)obj;
                    if (!string.IsNullOrEmpty(playlist.Id))
                    {
                        foreach (var plSong in await this.playlistsService.GetSongsAsync((IPlaylist)obj))
                        {
                            if (plSong.IsLibrary)
                            {
                                songs.Add(plSong);
                            }
                        }
                    }
                }
            }

            if (songs.Count > 0)
            {
                var yesUiCommand = new UICommand(this.applicationResources.GetString("MessageBox_DeletePlaylistYes"));
                var noUiCommand = new UICommand(this.applicationResources.GetString("MessageBox_DeletePlaylistNo"));

                MessageDialog dialog =
                    new MessageDialog(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Are you sure that you want to remove {0} song(s) from your library?",
                            songs.Count));

                dialog.Commands.Add(yesUiCommand);
                dialog.Commands.Add(noUiCommand);
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;

                var command = await dialog.ShowAsync();

                if (command == yesUiCommand)
                {
                    IList<Song> removedSongs = await this.songsService.RemoveFromLibraryAsync(songs);
                    return removedSongs != null;
                }
            }

            return null;
        }
    }
}
