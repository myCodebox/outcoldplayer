// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public class AddToLibraryAction : ISelectedObjectAction
    {
        private readonly IApplicationStateService stateService;

        private readonly IPlaylistsService playlistsService;

        private readonly ISongsService songsService;

        public AddToLibraryAction(
            IApplicationStateService stateService,
            IPlaylistsService playlistsService,
            ISongsService songsService)
        {
            this.stateService = stateService;
            this.playlistsService = playlistsService;
            this.songsService = songsService;
        }

        public string Icon
        {
            get
            {
                return CommandIcon.Add;
            }
        }

        public string Title
        {
            get
            {
                return "Add to library";
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
                return 1000;
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            if (!this.stateService.IsOnline())
            {
                return false;
            }

            bool hasNotLibrary = false;
            foreach (var obj in selectedObjects)
            {
                var song = obj as Song;
                if (song != null)
                {
                    if (!song.IsLibrary)
                    {
                        hasNotLibrary = true;
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
                        || playlist.PlaylistType == PlaylistType.SituationStations)
                    {
                        return false;
                    }

                    if (playlist.PlaylistType == PlaylistType.Artist && string.IsNullOrEmpty(playlist.Id))
                    {
                        return false;
                    }

                    hasNotLibrary |= string.IsNullOrEmpty(playlist.Id);
                }
            }

            return hasNotLibrary;
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            List<Song> songs = new List<Song>();

            foreach (var obj in selectedObjects)
            {
                var song = obj as Song;
                if (song != null)
                {
                    if (!song.IsLibrary)
                    {
                        songs.Add(song);
                    }
                }
                else
                {
                    var playlist = (IPlaylist)obj;
                    if (string.IsNullOrEmpty(playlist.Id))
                    {
                        songs.AddRange((await this.playlistsService.GetSongsAsync((IPlaylist)obj)).Where(x => !x.IsLibrary));
                    }
                }
            }

            IList<Song> addedSongs = await this.songsService.AddToLibraryAsync(songs);

            return addedSongs != null;
        }
    }
}
