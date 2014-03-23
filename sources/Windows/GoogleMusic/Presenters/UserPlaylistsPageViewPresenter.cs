// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class UserPlaylistsPageViewPresenter : PlaylistsPageViewPresenterBase<IUserPlaylistsPageView>
    {
        private readonly IAnalyticsService analyticsService;

        public UserPlaylistsPageViewPresenter(
            IApplicationResources resources,
            IPlaylistsService playlistsService,
            IAnalyticsService analyticsService)
            : base(resources, playlistsService)
        {
            this.analyticsService = analyticsService;
            this.AddPlaylistCommand = new DelegateCommand(this.AddPlaylist);
        }

        public DelegateCommand AddPlaylistCommand { get; private set; }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            yield return new CommandMetadata(CommandIcon.Page, "New playlist", this.AddPlaylistCommand);
        }

        private void AddPlaylist()
        {
            this.analyticsService.SendEvent("UserPlaylist", "Execute", "New Playlist");
            this.MainFrame.ShowPopup<IPlaylistEditPopupView>(PopupRegion.AppToolBarLeft, new UserPlaylist());
        }
    }
}
