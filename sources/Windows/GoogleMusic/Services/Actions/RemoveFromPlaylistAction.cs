// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------


namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Views;

    public class RemoveFromPlaylistAction : ISelectedObjectAction
    {
        private readonly IApplicationResources applicationResources;
        private readonly INavigationService navigationService;
        private readonly IUserPlaylistsService userPlaylistsService;

        public RemoveFromPlaylistAction(
            IApplicationResources applicationResources,
            INavigationService navigationService,
            IUserPlaylistsService userPlaylistsService)
        {
            this.applicationResources = applicationResources;
            this.navigationService = navigationService;
            this.userPlaylistsService = userPlaylistsService;
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
                return this.applicationResources.GetString("Toolbar_PlaylistButton");
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            IPageView currentView = this.navigationService.GetCurrentView();
            if (!(currentView is IPlaylistPageView))
            {
                return false;
            }

            var bindingModel = currentView.GetPresenter<PlaylistPageViewPresenter>().BindingModel;

            if (bindingModel == null)
            {
                return false;
            }

            var playlist = bindingModel.Playlist;
            return playlist is UserPlaylist && !((UserPlaylist)playlist).IsShared;
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            if (!this.CanExecute(selectedObjects))
            {
                return null;
            }

            var userPlaylist = (UserPlaylist)this.navigationService.GetCurrentView().GetPresenter<PlaylistPageViewPresenter>().BindingModel.Playlist;
            return await this.userPlaylistsService.RemoveSongsAsync(userPlaylist, selectedObjects.Cast<Song>());
        }
    }
}
