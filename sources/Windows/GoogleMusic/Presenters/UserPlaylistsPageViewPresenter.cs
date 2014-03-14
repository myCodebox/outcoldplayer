// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class UserPlaylistsPageViewPresenter : PlaylistsPageViewPresenterBase<IUserPlaylistsPageView, PlaylistsPageViewBindingModel>
    {
        public UserPlaylistsPageViewPresenter(
            IApplicationResources resources,
            IPlaylistsService playlistsService)
            : base(resources, playlistsService)
        {
            this.AddPlaylistCommand = new DelegateCommand(this.AddPlaylist);
        }

        public DelegateCommand AddPlaylistCommand { get; private set; }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            yield return new CommandMetadata(CommandIcon.Page, "New playlist", this.AddPlaylistCommand);
        }

        private void AddPlaylist()
        {
            this.MainFrame.ShowPopup<IPlaylistEditPopupView>(PopupRegion.AppToolBarLeft, new UserPlaylist());
        }
    }
}
