// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public class DeletePlaylistAction : ISelectedObjectAction
    {
        private readonly IApplicationResources applicationResources;
        private readonly IApplicationStateService stateService;
        private readonly IUserPlaylistsService userPlaylistsService;
        private readonly INotificationService notificationService;

        public DeletePlaylistAction(
            IApplicationResources applicationResources,
            IApplicationStateService stateService,
            IUserPlaylistsService userPlaylistsService,
            INotificationService notificationService)
        {
            this.applicationResources = applicationResources;
            this.stateService = stateService;
            this.userPlaylistsService = userPlaylistsService;
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

            bool? result = await this.notificationService.ShowQuestionAsync(
                this.applicationResources.GetString("MessageBox_DeletePlaylistMessage"),
                yesButton: this.applicationResources.GetString("MessageBox_DeletePlaylistYes"),
                noButton: this.applicationResources.GetString("MessageBox_DeletePlaylistNo"));

            if (result.HasValue && result.Value)
            {
                return await this.userPlaylistsService.DeleteAsync(selectedObjects.Cast<UserPlaylist>().ToList());
            }

            return null;
        }
    }
}
