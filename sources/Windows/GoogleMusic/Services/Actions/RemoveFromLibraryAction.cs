// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System;
    using System.Collections.Generic;
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
                return "Library";
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
                    if (playlist.PlaylistType == PlaylistType.Radio)
                    {
                        return false;
                    }

                    hasLibrary |= !string.IsNullOrEmpty(playlist.Id) && !(playlist is UserPlaylist);
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
                    if (!string.IsNullOrEmpty(playlist.Id) && !(playlist is UserPlaylist))
                    {
                        songs.AddRange((await this.playlistsService.GetSongsAsync((IPlaylist)obj)).Where(x => x.IsLibrary));
                    }
                }
            }

            IList<Song> removedSongs = await this.songsService.RemoveFromLibraryAsync(songs);

            return removedSongs != null;
        }
    }
}
