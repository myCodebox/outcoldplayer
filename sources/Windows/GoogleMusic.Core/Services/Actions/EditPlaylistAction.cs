// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters.Popups;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class EditPlaylistAction : ISelectedObjectAction
    {
        private readonly IApplicationStateService stateService;

        private readonly IMainFrame mainFrame;

        private TaskCompletionSource<bool?> taskCompletionSource;

        public EditPlaylistAction(
            IApplicationStateService stateService,
            IMainFrame mainFrame)
        {
            this.stateService = stateService;
            this.mainFrame = mainFrame;
        }

        public string Icon
        {
            get
            {
                return CommandIcon.Edit;
            }
        }

        public string Title
        {
            get
            {
                return "Edit playlist name";
            }
        }

        public ActionGroup Group
        {
            get
            {
                return ActionGroup.Playlists;
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

            if (selectedObjects.Count != 1)
            {
                return false;
            }

            return (selectedObjects[0] is UserPlaylist) && !((UserPlaylist)selectedObjects[0]).IsShared;
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            if (!this.CanExecute(selectedObjects))
            {
                return false;
            }

            var source = this.taskCompletionSource = new TaskCompletionSource<bool?>();
            this.mainFrame.ShowPopup<IPlaylistEditPopupView>(PopupRegion.AppToolBarLeft, selectedObjects[0]).Closed += this.OnClosed;
            return await source.Task;
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            ((IPlaylistEditPopupView)sender).Closed -= this.OnClosed;
            var source = this.taskCompletionSource;
            this.taskCompletionSource = null;
            source.SetResult(eventArgs is PlaylistEditCompletedEventArgs ? (bool?) true : null);
        }
    }
}
