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

    internal class DeleteRadioStationsAction : ISelectedObjectAction
    {
        private readonly IApplicationResources applicationResources;

        private readonly IApplicationStateService stateService;

        private readonly IRadioStationsService radioStationsService;

        public DeleteRadioStationsAction(
            IApplicationResources applicationResources,
            IApplicationStateService stateService,
            IRadioStationsService radioStationsService)
        {
            this.applicationResources = applicationResources;
            this.stateService = stateService;
            this.radioStationsService = radioStationsService;
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
                return this.applicationResources.GetString("Toolbar_DeleteButton");
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            if (!this.stateService.IsOnline())
            {
                return false;
            }

            return selectedObjects.All(x => x is Radio && !string.IsNullOrEmpty(((Radio)x).Id));
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            if (!this.CanExecute(selectedObjects))
            {
                return null;
            }

            var yesUiCommand = new UICommand(this.applicationResources.GetString("MessageBox_DeletePlaylistYes"));
            var noUiCommand = new UICommand(this.applicationResources.GetString("MessageBox_DeletePlaylistNo"));

            MessageDialog dialog = new MessageDialog(this.applicationResources.GetString("MessageBox_DeleteRadioMessage"));
            dialog.Commands.Add(yesUiCommand);
            dialog.Commands.Add(noUiCommand);
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            var command = await dialog.ShowAsync();

            if (command == yesUiCommand)
            {
                return await this.radioStationsService.DeleteAsync(selectedObjects.Cast<Radio>().ToList());
            }

            return null;
        }
    }
}
