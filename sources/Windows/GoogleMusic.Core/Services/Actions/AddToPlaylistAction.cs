// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class AddToPlaylistAction : ISelectedObjectAction
    {
        private readonly IApplicationStateService stateService;
        private readonly IPlaylistsService playlistsService;
        private readonly IMainFrame mainFrame;

        private TaskCompletionSource<bool?> taskCompletionSource;

        public AddToPlaylistAction(
            IApplicationStateService stateService,
            IPlaylistsService playlistsService,
            IMainFrame mainFrame)
        {
            this.stateService = stateService;
            this.playlistsService = playlistsService;
            this.mainFrame = mainFrame;
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
                return "Add to playlist";
            }
        }

        public ActionGroup Group
        {
            get
            {
                return ActionGroup.Playlist;
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

            foreach (var obj in selectedObjects)
            {   
                if (!(obj is Song))
                {
                    var playlist = (IPlaylist)obj;
                    if (playlist.PlaylistType == PlaylistType.Radio
                        || playlist.PlaylistType == PlaylistType.Situation
                        || playlist.PlaylistType == PlaylistType.SituationStations
                        || playlist.PlaylistType == PlaylistType.AllAccessGenre)
                    {
                        return false;
                    }

                    if (playlist.PlaylistType == PlaylistType.Artist && string.IsNullOrEmpty(playlist.Id))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            List<Song> songs = new List<Song>();

            foreach (var obj in selectedObjects)
            {
                var song = obj as Song;
                if (song != null)
                {
                    songs.Add(song);
                }
                else
                {
                    songs.AddRange(await this.playlistsService.GetSongsAsync((IPlaylist)obj));
                }
            }

            if (songs.Count > 0)
            {
                TaskCompletionSource<bool?> source = this.taskCompletionSource = new TaskCompletionSource<bool?>();

                this.mainFrame.ShowPopup<IAddToPlaylistPopupView>(
                    PopupRegion.AppToolBarLeft,
                    songs).Closed += this.AddToPlaylist_Closed;

                return await source.Task;
            }

            return null;
        }

        private void AddToPlaylist_Closed(object sender, EventArgs eventArgs)
        {
            ((IPopupView)sender).Closed -= this.AddToPlaylist_Closed;

            var source = this.taskCompletionSource;
            this.taskCompletionSource = null;
            source.SetResult(eventArgs is AddToPlaylistCompletedEventArgs ? (bool?)true : null);
        }
    }
}
