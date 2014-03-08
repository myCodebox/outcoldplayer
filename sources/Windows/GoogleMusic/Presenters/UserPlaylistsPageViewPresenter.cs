// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class UserPlaylistsPageViewPresenter : PlaylistsPageViewPresenterBase<IUserPlaylistsPageView, PlaylistsPageViewBindingModel>
    {
        private readonly IApplicationResources resources;
        private readonly INavigationService navigationService;
        private readonly IPlayQueueService playQueueService;

        public UserPlaylistsPageViewPresenter(
            IApplicationResources resources,
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            IPlaylistsService playlistsService,
            ISelectedObjectsService selectedObjectsService)
            : base(resources, playlistsService, navigationService, playQueueService, selectedObjectsService)
        {
            this.resources = resources;
            this.navigationService = navigationService;
            this.playQueueService = playQueueService;
            this.AddPlaylistCommand = new DelegateCommand(this.AddPlaylist);
        }

        public DelegateCommand AddPlaylistCommand { get; private set; }

        public void PlaySharedPlaylist(IPlaylist playlist)
        {
            if (this.PlayCommand.CanExecute(playlist) && ((UserPlaylist)playlist).IsShared)
            {
                var currentPlaylist = this.playQueueService.CurrentPlaylist;

                if (currentPlaylist != null
                    && currentPlaylist.PlaylistType == PlaylistType.UserPlaylist
                    && string.Equals(currentPlaylist.Id, playlist.Id, StringComparison.Ordinal))
                {
                    this.navigationService.NavigateTo<ICurrentPlaylistPageView>();
                }
                else
                {
                    this.PlayCommand.Execute(playlist);
                }
            }
        }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            yield return new CommandMetadata(CommandSymbol.Page, "New playlist", this.AddPlaylistCommand);
        }

        protected async override Task LoadPlaylistsAsync()
        {
            await base.LoadPlaylistsAsync();
            await this.Dispatcher.RunAsync(() => this.BindingModel.ClearSelectedItems());
        }

        private void AddPlaylist()
        {
            this.MainFrame.ShowPopup<IPlaylistEditPopupView>(PopupRegion.AppToolBarLeft, new UserPlaylist());
        }
    }
}
