// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public class DeleteRadioStationsAction : ISelectedObjectAction
    {
        private readonly IApplicationResources applicationResources;

        private readonly IApplicationStateService stateService;

        private readonly IRadioStationsService radioStationsService;

        private readonly ISettingsService settingsService;
        private readonly INotificationService notificationService;

        public DeleteRadioStationsAction(
            IApplicationResources applicationResources,
            IApplicationStateService stateService,
            IRadioStationsService radioStationsService,
            ISettingsService settingsService,
            INotificationService notificationService)
        {
            this.applicationResources = applicationResources;
            this.stateService = stateService;
            this.radioStationsService = radioStationsService;
            this.settingsService = settingsService;
            this.notificationService = notificationService;
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
                return this.settingsService.GetIsAllAccessAvailable() ? "Delete radio station(s)" : "Delete instant mixes";
            }
        }

        public ActionGroup Group
        {
            get
            {
                return ActionGroup.RadioStations;
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

            return selectedObjects.All(x => x is Radio && ((Radio)x).PlaylistType == PlaylistType.Radio && !string.IsNullOrEmpty(((Radio)x).Id));
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            if (!this.CanExecute(selectedObjects))
            {
                return null;
            }

            bool? result = await this.notificationService.ShowQuestionAsync(
                this.settingsService.GetIsAllAccessAvailable()
                    ? "Are you sure want to delete selected radio stations?"
                    : "Are you sure want to delete selected instant mixes?",
                yesButton: this.applicationResources.GetString("MessageBox_DeletePlaylistYes"),
                noButton: this.applicationResources.GetString("MessageBox_DeletePlaylistNo"));

            if (result.HasValue && result.Value)
            {
                return await this.radioStationsService.DeleteAsync(selectedObjects.Cast<Radio>().ToList());
            }

            return null;
        }
    }
}
