﻿// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public class RemoveLocalAction : ISelectedObjectAction
    {
        private readonly IApplicationResources applicationResources;
        private readonly IPlaylistsService playlistsService;
        private readonly ISongsCachingService songsCachingService;

        public RemoveLocalAction(
            IApplicationResources applicationResources,
            IPlaylistsService playlistsService,
            ISongsCachingService songsCachingService)
        {
            this.applicationResources = applicationResources;
            this.playlistsService = playlistsService;
            this.songsCachingService = songsCachingService;
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
                return this.applicationResources.GetString("Toolbar_RemoveLocal");
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            foreach (var selectedObject in selectedObjects)
            {
                var song = selectedObject as Song;
                if (song != null && song.IsCached)
                {
                    return true;
                }

                var playlist = selectedObject as IPlaylist;
                if (playlist != null && playlist.OfflineSongsCount > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            List<Song> songs = new List<Song>();

            foreach (var selectedObject in selectedObjects)
            {
                var song = selectedObject as Song;
                if (song != null && song.IsCached)
                {
                    songs.Add(song);
                }
                
                var playlist = selectedObject as IPlaylist;
                if (playlist != null && playlist.OfflineSongsCount > 0)
                {
                    songs.AddRange((await this.playlistsService.GetSongsAsync(playlist)).Where(x => x.IsCached));
                }
            }

            if (songs.Count > 0)
            {
                await this.songsCachingService.ClearCachedAsync(songs);
            }

            return true;
        }
    }
}