// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Views;

    using Windows.UI.Popups;

    public class UserPlaylistsPageViewPresenter : PlaylistsPageViewPresenterBase<IUserPlaylistsPageView, PlaylistsPageViewBindingModel>
    {
        private readonly IApplicationResources resources;
        private readonly IUserPlaylistsService userPlaylistsService;
        
        public UserPlaylistsPageViewPresenter(
            IApplicationResources resources,
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            IPlaylistsService playlistsService,
            IUserPlaylistsService userPlaylistsService)
            : base(resources, playlistsService, navigationService, playQueueService)
        {
            this.resources = resources;
            this.userPlaylistsService = userPlaylistsService;
            this.AddPlaylistCommand = new DelegateCommand(this.AddPlaylist);
            this.EditPlaylistCommand = new DelegateCommand(this.EditPlaylist, () => this.BindingModel.SelectedItems.Count == 1);
            this.DeletePlaylistsCommand = new DelegateCommand(this.DeletePlaylists, () => this.BindingModel.SelectedItems.Count > 0);
        }

        public DelegateCommand AddPlaylistCommand { get; private set; }

        public DelegateCommand DeletePlaylistsCommand { get; private set; }

        public DelegateCommand EditPlaylistCommand { get; private set; }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            yield return new CommandMetadata(CommandIcon.Page, this.resources.GetString("Toolbar_CreateButton"), this.AddPlaylistCommand);
        }

        protected async override Task LoadPlaylistsAsync()
        {
            await base.LoadPlaylistsAsync();
            await this.Dispatcher.RunAsync(() => this.BindingModel.ClearSelectedItems());
        }

        protected override void OnSelectedItemsChanged()
        {
            base.OnSelectedItemsChanged();

            this.AddPlaylistCommand.RaiseCanExecuteChanged();
            this.DeletePlaylistsCommand.RaiseCanExecuteChanged();
            this.EditPlaylistCommand.RaiseCanExecuteChanged();
        }

        protected override IEnumerable<CommandMetadata> GetContextCommands()
        {
            foreach (CommandMetadata commandMetadata in base.GetContextCommands())
            {
                yield return commandMetadata;
            }

            yield return new CommandMetadata(CommandIcon.Edit, this.resources.GetString("Toolbar_RenameButton"), this.EditPlaylistCommand);
            yield return new CommandMetadata(CommandIcon.Delete, this.resources.GetString("Toolbar_DeleteButton"), this.DeletePlaylistsCommand);
        }

        private void AddPlaylist()
        {
            this.MainFrame.ShowPopup<IPlaylistEditPopupView>(PopupRegion.AppToolBarLeft, new UserPlaylist());
        }

        private void EditPlaylist()
        {
            if (this.EditPlaylistCommand.CanExecute())
            {
                this.MainFrame.ShowPopup<IPlaylistEditPopupView>(PopupRegion.AppToolBarLeft, this.BindingModel.SelectedItems[0].Playlist);
            }
        }

        private async void DeletePlaylists()
        {
            if (this.DeletePlaylistsCommand.CanExecute())
            {
                var yesUiCommand = new UICommand(this.resources.GetString("MessageBox_DeletePlaylistYes"));
                var noUiCommand = new UICommand(this.resources.GetString("MessageBox_DeletePlaylistNo"));

                var playlists = this.BindingModel.SelectedItems.Select(bm => bm.Playlist).ToList();

                MessageDialog dialog = new MessageDialog(this.resources.GetString("MessageBox_DeletePlaylistMessage"));
                dialog.Commands.Add(yesUiCommand);
                dialog.Commands.Add(noUiCommand);
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                var command = await dialog.ShowAsync();

                if (command == yesUiCommand)
                {
                    foreach (UserPlaylist playlist in playlists)
                    {
                        await this.userPlaylistsService.DeleteAsync(playlist);
                    }
                }
            }
        }
    }
}
