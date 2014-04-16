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

    public class DeletePlaylistAction : ISelectedObjectAction
    {
        private readonly IApplicationResources applicationResources;
        private readonly IApplicationStateService stateService;
        private readonly IUserPlaylistsService userPlaylistsService;

        public DeletePlaylistAction(
            IApplicationResources applicationResources,
            IApplicationStateService stateService,
            IUserPlaylistsService userPlaylistsService)
        {
            this.applicationResources = applicationResources;
            this.stateService = stateService;
            this.userPlaylistsService = userPlaylistsService;
        }

        public string Icon
        {
            get
            {
                return CommandIcon.Delete;
            }
        }

        public string Title
        {
            get
            {
                return "Delete playlist(s)";
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
                return 500;
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            if (!this.stateService.IsOnline())
            {
                return false;
            }

            return selectedObjects.All(x => x is UserPlaylist);
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            if (!this.CanExecute(selectedObjects))
            {
                return null;
            }

            var yesUiCommand = new UICommand(this.applicationResources.GetString("MessageBox_DeletePlaylistYes"));
            var noUiCommand = new UICommand(this.applicationResources.GetString("MessageBox_DeletePlaylistNo"));

            MessageDialog dialog = new MessageDialog(this.applicationResources.GetString("MessageBox_DeletePlaylistMessage"));
            dialog.Commands.Add(yesUiCommand);
            dialog.Commands.Add(noUiCommand);
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            var command = await dialog.ShowAsync();

            if (command == yesUiCommand)
            {
                return await this.userPlaylistsService.DeleteAsync(selectedObjects.Cast<UserPlaylist>().ToList());
            }

            return null;
        }
    }
}
