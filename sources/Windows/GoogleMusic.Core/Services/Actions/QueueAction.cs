// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class QueueAction : ISelectedObjectAction
    {
        private readonly IApplicationResources applicationResources;
        private readonly IMainFrame mainFrame;

        private TaskCompletionSource<bool?> taskCompletionSource;

        public QueueAction(
            IApplicationResources applicationResources,
            IMainFrame mainFrame)
        {
            this.applicationResources = applicationResources;
            this.mainFrame = mainFrame;
        }

        public string Icon
        {
            get
            {
                return CommandIcon.OpenWith;
            }
        }

        public string Title
        {
            get
            {
                return this.applicationResources.GetString("Toolbar_QueueButton");
            }
        }

        public ActionGroup Group
        {
            get
            {
                return ActionGroup.Navigation;
            }
        }

        public int Priority
        {
            get
            {
                return 2000;
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            foreach (var selectedObject in selectedObjects)
            {
                if (!(selectedObject is Song))
                {
                    var playlist = (IPlaylist)selectedObject;
                    if (playlist.PlaylistType == PlaylistType.Radio
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
                }
            }

            return true;
        }

        public Task<bool?> Execute(IList<object> selectedObjects)
        {
            this.taskCompletionSource = new TaskCompletionSource<bool?>();

            SelectedItems selectedItems = new SelectedItems(
                selectedObjects.Where(x => x is IPlaylist).Cast<IPlaylist>().ToList(), 
                selectedObjects.Where(x => x is Song).Cast<Song>().ToList());

            this.mainFrame.ShowPopup<IQueueActionsPopupView>(
                PopupRegion.AppToolBarLeft,
                selectedItems).Closed += this.QueueActionsPopupView_Closed;

            return taskCompletionSource.Task;
        }

        private void QueueActionsPopupView_Closed(object sender, EventArgs eventArgs)
        {
            ((IPopupView)sender).Closed -= this.QueueActionsPopupView_Closed;

            var source = this.taskCompletionSource;
            this.taskCompletionSource = null;

            source.SetResult(eventArgs is QueueActionsCompletedEventArgs ? (bool?) true : null);
        }
    }
}
