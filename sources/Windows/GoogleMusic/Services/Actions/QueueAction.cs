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

        public bool CanExecute(IList<object> selectedObjects)
        {
            return selectedObjects.Count > 0 && 
                (
                    selectedObjects.All(x => x is Song && !((Song)x).UnknownSong) || 
                    selectedObjects.All(
                        x => x is IPlaylist && 
                            ( 
                                ((IPlaylist)x).PlaylistType != PlaylistType.Radio) && 
                                ((((IPlaylist)x).PlaylistType != PlaylistType.UserPlaylist) || !((UserPlaylist)x).IsShared)
                            )
                );
        }

        public Task<bool?> Execute(IList<object> selectedObjects)
        {
            this.taskCompletionSource = new TaskCompletionSource<bool?>();

            SelectedItems selectedItems = null;

            if (selectedObjects.All(x => x is Song))
            {
                selectedItems = new SelectedItems(selectedObjects.Cast<Song>().ToList());
            }
            else
            {
                selectedItems = new SelectedItems(selectedObjects.Cast<IPlaylist>().ToList());
            }

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
